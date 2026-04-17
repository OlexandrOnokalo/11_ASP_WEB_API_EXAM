import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import CardActions from "@mui/material/CardActions";
import Typography from "@mui/material/Typography";
import { useDispatch } from "react-redux";
import axios from "axios";
import { Button } from "@mui/material";
import { Link } from "react-router-dom";

const CarCard = ({ car, onDelete }) => {
    const dispatch = useDispatch();
    const deleteClickHandle = async () => {
        if (onDelete) {
            onDelete(car.id);
            return;
        }
        if(!confirm('Видалити автомобіль?')) return;
        try {
            await axios.delete(`${import.meta.env.VITE_CARS_URL}/${car.id}`);
            dispatch({ type: 'deletecar', payload: car.id });
        } catch(e){ console.error(e) }
    };
    return (
        <Card>
            <CardContent>
                <img style={{ height: '150 px' }} src={car.image} alt={car.name} />
                <Typography variant="h6">{car.name}{car.manufacture?.name ? ` — ${car.manufacture.name}`: ''}</Typography>
                <Typography>Рік: {car.year} • Об'єм: {car.volume} • Ціна: {car.price}</Typography>
                <Typography>Колір: {car.color}</Typography>
                <Typography variant="body2" sx={{ mt:1 }}>{car.description}</Typography>
            </CardContent>
            <CardActions>
                <Button size="small" component={Link} to={`/cars/${car.id}`}>Деталі</Button>
                <Button size="small" component={Link} to={`/cars/update/${car.id}`}>Редагувати</Button>
                <Button size="small" color="error" onClick={deleteClickHandle}>Видалити</Button>
            </CardActions>
        </Card>
    );
};

export default CarCard;
