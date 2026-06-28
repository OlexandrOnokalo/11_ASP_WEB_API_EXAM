import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import CardActions from "@mui/material/CardActions";
import Typography from "@mui/material/Typography";
import { useDispatch } from "react-redux";
import { Button } from "@mui/material";
import { Link } from "react-router-dom";
import { api } from "../../api";
import { toImageSrc } from "../../services/imageUrl";

const CarCard = ({ car, onDelete, canManage }) => {
    const dispatch = useDispatch();
    const deleteClickHandle = async () => {
        // Якщо батьківський передав onDelete — делегуюю йому,
        // інакше роблю DELETE сам і диспатчу оновлення store
        if (onDelete) {
            onDelete(car.id);
            return;
        }
        if(!confirm('Видалити автомобіль?')) return;
        try {
            await api.delete(`cars/${car.id}`);
            // Оновлюю store локально — не роблю повторний GET після DELETE
            dispatch({ type: 'deletecar', payload: car.id });
        } catch(e){ console.error(e) }
    };
    return (
        <Card>
            <CardContent>
                <img
                    style={{ maxHeight: '200px', height: '200px', width: '100%', objectFit: 'contain' }}
                    src={toImageSrc(car.image)}
                    alt={car.name}
                />
                <Typography variant="h6">{car.name}{car.manufacture?.name ? ` — ${car.manufacture.name}`: ''}</Typography>
                <Typography>Рік: {car.year} • Об'єм: {car.volume} • Ціна: {car.price}</Typography>
                <Typography>Колір: {car.color}</Typography>
                <Typography variant="body2" sx={{ mt:1 }}>{car.description}</Typography>
            </CardContent>
            <CardActions>
                <Button size="small" component={Link} to={`/cars/${car.id}`}>Деталі</Button>
                {/* canManage = isAdmin — приходить з CarListPage */}
                {canManage && <Button size="small" component={Link} to={`/cars/update/${car.id}`}>Редагувати</Button>}
                {canManage && <Button size="small" color="error" onClick={deleteClickHandle}>Видалити</Button>}
            </CardActions>
        </Card>
    );
};

export default CarCard;
