import { Button } from "@mui/material";
import { Link } from "react-router-dom";

const MainPage = () => {
    return (
        <>
            <h1 style={{ textAlign: "center" }}>Головна сторінка</h1>
            <div style={{ display: "flex", justifyContent: "center" }}>
                <Link to="/cars">
                    <Button variant="contained" sx={{ mx: 1 }}>
                        Автомобілі
                    </Button>
                </Link>

                <Link to="/Manufactures">
                    <Button variant="contained" sx={{ mx: 1 }}>
                        Виробники
                    </Button>
                </Link>
            </div>
        </>
    );
};

export default MainPage;
