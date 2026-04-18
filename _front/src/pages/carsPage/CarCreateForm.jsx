import Stack from "@mui/material/Stack";
import MuiCard from "@mui/material/Card";
import { styled } from "@mui/material/styles";
import { useNavigate } from "react-router-dom";
import { useFormik } from "formik";
import { object, number, string } from "yup";
import { useEffect, useState } from "react";
import { useDispatch } from "react-redux";
import { api } from "../../api";
import { getItems } from "../../services/responseParsers";

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

export default function CarCreateForm(){
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const [manufactures, setManufactures] = useState([]);
    useEffect(()=> {
        api.get("manufactures", { params: { page:1, page_size:200 }}).then(r=>{
            setManufactures(getItems(r));
        }).catch(()=>setManufactures([]));
    },[]);
    const formik = useFormik({
        initialValues: { name:'', manufactureId:0, year:new Date().getFullYear(), volume:1.6, price:0, color:'', description:'', image: null },
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

                await api.post("cars", formData);
                const listRes = await api.get("cars", { params: { page: 1, page_size: 100 } });
                dispatch({ type: 'loadcars', payload: getItems(listRes) });
                navigate('/cars');
            } catch(e){
                console.error(e);
            }
        }
    });
    return (
        <Card>
            <h2>Додати автомобіль</h2>
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
