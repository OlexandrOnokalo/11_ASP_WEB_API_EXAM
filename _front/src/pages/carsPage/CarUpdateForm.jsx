import Stack from "@mui/material/Stack";
import MuiCard from "@mui/material/Card";
import { styled } from "@mui/material/styles";
import { useNavigate, useParams } from "react-router-dom";
import { useFormik } from "formik";
import { object, number, string } from "yup";
import { useEffect, useState } from "react";
import { useDispatch } from "react-redux";
import { api } from "../../api";
import { getEntity, getItems } from "../../services/responseParsers";
import { toImageSrc } from "../../services/imageUrl";

const Card = styled(MuiCard)(({ theme }) => ({
    display: "flex",
    flexDirection: "column",
    alignSelf: "center",
    width: "100%",
    padding: 16
}));

const schema = object({
    name: string().required(),
    year: number().required(),
    volume: number().required(),
    price: number().required(),
    color: string().required()
});

export default function CarUpdateForm(){
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const { id } = useParams();
    const [manufactures, setManufactures] = useState([]);
    const [initial, setInitial] = useState(null);

    useEffect(()=> {
        api.get("manufactures", { params: { page:1, page_size:200 }}).then(r=>{
            setManufactures(getItems(r));
        }).catch(()=>setManufactures([]));

        if(id){
            api.get(`cars/${id}`).then(r=>{
                setInitial(getEntity(r));
            }).catch(()=>setInitial(null));
        }
    },[id]);

    const formik = useFormik({
        enableReinitialize: true,
        initialValues: initial ? {
            name: initial.name || '',
            manufactureId: initial.manufacture?.id || 0,
            year: initial.year || new Date().getFullYear(),
            volume: initial.volume || 1.6,
            price: initial.price || 0,
            color: initial.color || '',
            description: initial.description || initial.desciption || '',
            image: null
        } : { name:'', manufactureId:0, year:new Date().getFullYear(), volume:1.6, price:0, color:'', description:'', image:null },
        validationSchema: schema,
        onSubmit: async (values) => {
            try {
                const formData = new FormData();
                formData.append("name", values.name);
                formData.append("manufactureId", String(Number(values.manufactureId)));
                formData.append("year", String(Number(values.year)));
                formData.append("volume", String(Number(values.volume)));
                formData.append("price", String(Number(values.price)));
                formData.append("color", values.color);
                formData.append("description", values.description || "");
                formData.append("desciption", values.description || "");
                if (values.image instanceof File) {
                    formData.append("image", values.image);
                }

                await api.put(`cars/${id}`, formData);
                const listRes = await api.get("cars", { params: { page: 1, page_size: 100 } });
                dispatch({ type: 'loadcars', payload: getItems(listRes) });
                navigate(`/cars/${id}`);
            } catch(e){
                console.error(e);
            }
        }
    });
    return (
        <Card>
            <h2>Редагувати автомобіль</h2>
            <form onSubmit={formik.handleSubmit}>
                <Stack spacing={2}>
                    <input name="name" placeholder="Назва" value={formik.values.name} onChange={formik.handleChange} />
                    <select name="manufactureId" value={formik.values.manufactureId} onChange={formik.handleChange}>
                        <option value={0}>Оберіть виробника</option>
                        {manufactures.map(m=> <option key={m.id} value={m.id}>{m.name}</option>)}
                    </select>
                    <input name="year" placeholder="Рік" value={formik.values.year} onChange={formik.handleChange} />
                    <input name="volume" placeholder="Об'єм" value={formik.values.volume} onChange={formik.handleChange} />
                    <input name="price" placeholder="Ціна" value={formik.values.price} onChange={formik.handleChange} />
                    <input name="color" placeholder="Колір" value={formik.values.color} onChange={formik.handleChange} />
                    <input name="description" placeholder="Опис" value={formik.values.description} onChange={formik.handleChange} />
                    {initial?.image && <img src={toImageSrc(initial.image)} alt={initial.name} style={{ width: "220px", height: "140px", objectFit: "cover" }} />}
                    <input
                        type="file"
                        name="image"
                        accept="image/*"
                        onChange={(e) => formik.setFieldValue("image", e.target.files?.[0] || null)}
                    />
                    <button type="submit">Зберегти</button>
                </Stack>
            </form>
        </Card>
    );
}
