import { useEffect, useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import { Box, Card, CardContent, Typography, Button } from "@mui/material";
import axios from "axios";
import { useAuth } from "../../context/AuthContext";

const CarDetailsPage = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const { isAuth } = useAuth();
    const [car, setCar] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetch = async () => {
            setLoading(true);
            try {
                const res = await axios.get(`${import.meta.env.VITE_CARS_URL}/${id}`);
                const data = res.data && res.data.data ? res.data.data : res.data;
                setCar(data);
            } catch (e) {
                console.error(e);
                setCar(null);
            } finally {
                setLoading(false);
            }
        };
        if (id) fetch();
    }, [id]);

    const onDelete = async () => {
        if (!confirm("Видалити автомобіль?")) return;
        try {
            await axios.delete(`${import.meta.env.VITE_CARS_URL}/${id}`);
            navigate("/cars");
        } catch (e) {
            console.error(e);
        }
    };

    if (loading) return <div>Завантаження...</div>;
    if (!car) return <div>Автомобіль не знайдено</div>;

    return (
        <Box sx={{ maxWidth: 900, margin: "24px auto" }}>
            <Card>
                <CardContent>
                    <Typography variant="h5" gutterBottom>
                        {car.name} {car.manufacture?.name ? ` — ${car.manufacture.name}` : ""}
                    </Typography>
                    <Typography>Рік: {car.year}</Typography>
                    <Typography>Об'єм: {car.volume}</Typography>
                    <Typography>Колір: {car.color}</Typography>
                    <Typography>Ціна: {car.price}</Typography>
                    <Typography sx={{ mt: 2 }}>{car.description}</Typography>

                    <Box sx={{ mt: 2, display: "flex", gap: 1, flexWrap: 'wrap' }}>
                        <Link to="/cars">
                            <Button variant="contained">Назад до списку</Button>
                        </Link>

                        <Link to={`/cars?manufactureId=${car.manufacture?.id || ""}`}>
                            <Button variant="contained">Усі від виробника</Button>
                        </Link>

                        <Link to={`/cars?year=${car.year}`}>
                            <Button variant="contained">Усі за роком</Button>
                        </Link>

                        <Link to={`/cars?color=${encodeURIComponent(car.color)}`}>
                            <Button variant="contained">Усі за кольором</Button>
                        </Link>

                        <Link to={`/cars?volume=${car.volume}`}>
                            <Button variant="contained">Усі за об'ємом</Button>
                        </Link>

                        <Link to={`/cars?minValue=${car.price - 1000}&maxValue=${car.price + 1000}`}>
                            <Button variant="contained">Похожие по ціні</Button>
                        </Link>

                        {isAuth && (
                            <>
                                <Link to={`/cars/update/${car.id}`}>
                                    <Button variant="contained" color="warning">Редагувати</Button>
                                </Link>
                                <Button variant="contained" color="error" onClick={onDelete}>
                                    Видалити
                                </Button>
                            </>
                        )}
                    </Box>
                </CardContent>
            </Card>
        </Box>
    );
};

export default CarDetailsPage;
