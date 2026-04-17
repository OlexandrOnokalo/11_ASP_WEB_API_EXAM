import Stack from "@mui/material/Stack";
import MuiCard from "@mui/material/Card";
import { styled } from "@mui/material/styles";
import { useNavigate, useParams } from "react-router-dom";
import { useFormik } from "formik";
import { object, number, string } from "yup";
import { useEffect, useState } from "react";
import axios from "axios";
import { useDispatch } from "react-redux";

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
        axios.get(import.meta.env.VITE_MANUFACTURES_URL, { params: { page:1, page_size:200 }}).then(r=>{
            const data = r.data && r.data.data ? r.data.data : r.data;
            setManufactures(data.items || data || []);
        }).catch(()=>setManufactures([]));
    },[]);
    const formik = useFormik({
        initialValues: { name:'', manufactureId:0, year:new Date().getFullYear(), volume:1.6, price:0, color:'', description:'', image: '' },
        validationSchema: schema,
        onSubmit: async (values) => {
            try {
                const response = await axios.post(import.meta.env.VITE_CARS_URL, {
                    name: values.name,
                    manufactureId: Number(values.manufactureId),
                    year: Number(values.year),
                    volume: Number(values.volume),
                    price: Number(values.price),
                    color: values.color,
                    description: values.description,
                    image: values.image
                });
                const listRes = await axios.get(import.meta.env.VITE_CARS_URL, { params: { page: 1, page_size: 100 } });
                const data = listRes.data && listRes.data.data ? listRes.data.data : listRes.data;
                if(data && data.items) {
                    dispatch({ type: 'loadcars', payload: data.items });
                } else if(Array.isArray(data)) {
                    dispatch({ type: 'loadcars', payload: data });
                }
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
                    <input name="image" placeholder="URL зображення" value={formik.values.image} onChange={formik.handleChange} />
                    <button type="submit">Зберегти</button>
                </Stack>
            </form>
        </Card>
    );
}
