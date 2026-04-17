import Card from '@mui/material/Card';
import CardActions from '@mui/material/CardActions';
import CardContent from '@mui/material/CardContent';
import CardMedia from '@mui/material/CardMedia';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import CardHeader from '@mui/material/CardHeader';
import IconButton from '@mui/material/IconButton';
import { red } from '@mui/material/colors';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import { Link } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import axios from 'axios';

const ManufacturesCard = ({ manufacture }) => {
    const dispatch = useDispatch();

    const deleteClickHandle = async () => {
        const ManufacturesUrl = import.meta.env.VITE_MANUFACTURES_URL;
        try {
            await axios.delete(`${ManufacturesUrl}/${manufacture.id}`);
            dispatch({ type: "deletemanufacture", payload: manufacture.id });
        } catch (error) {
            console.log(error);
        }
    };

    return (
        <Card sx={{ maxWidth: 345, height: "100%" }}>
            <CardHeader
                action={
                    <IconButton
                        onClick={deleteClickHandle}
                        color="error"
                        aria-label="settings"
                    >
                        <DeleteIcon />
                    </IconButton>
                }
                title={manufacture.name}
            />

            <CardActions disableSpacing>
                <Link to={`update/${manufacture.id}`}>
                    <IconButton color="success" aria-label="edit">
                        <EditIcon />
                    </IconButton>
                </Link>

                <Link to={`/cars?manufactureId=${manufacture.id}`}>
                    <Button size="small">Переглянути авто</Button>
                </Link>
            </CardActions>
        </Card>
    );
};

export default ManufacturesCard;
