import ManufacturesCard from "./ManufacturesCard";
import { Box, Grid, IconButton, CircularProgress } from "@mui/material";
import AddCircleIcon from "@mui/icons-material/AddCircle";
import { Link } from "react-router-dom";
import axios from "axios";
import { useAuth } from "../../context/AuthContext";
import { useDispatch, useSelector } from "react-redux";
import { useState, useEffect } from "react";

const ManufacturesListPage = () => {
    const dispatch = useDispatch();
    const { isAuth, user } = useAuth();


    const { Manufactures, isLoaded } = useSelector((state) => state.manufacture);

    async function fetchManufactures() {
        const ManufacturesUrl = import.meta.env.VITE_MANUFACTURES_URL;
        const pageCount = 150;
        const page = 1;
        const url = `${ManufacturesUrl}?page_size=${pageCount}&page=${page}`;

        if (!isLoaded) {
            const response = await axios.get(url);
            const { data, status } = response;
            if (status === 200) {
                const ManufacturesData = data.data.items;
                dispatch({ type: "loadManufactures", payload: ManufacturesData });
            } else {
                console.log("Не вдалося завантажити авторів");
            }
        }
    }

    useEffect(() => {
        fetchManufactures();
    }, []);

    if (!isLoaded) {
        return (
            <Box sx={{ display: "flex", justifyContent: "center" }}>
                <CircularProgress enableTrackSlot size="3rem" sx={{ mt: 4 }} />
            </Box>
        );
    }

    return (
        <Box
            sx={{
                display: "flex",
                alignItems: "center",
                flexDirection: "column",
            }}
        >
            <Grid container spacing={2} mx="100px" my="50px">
                {Manufactures.map((a, index) => (
                    <Grid size={4} key={index}>
                        <ManufacturesCard manufacture={a} />
                    </Grid>
                ))}
                {isAuth && user.role === "admin" && (
                    <Grid size={Manufactures.length % 3 === 0 ? 12 : 4}>
                        <Box
                            width="100%"
                            display="flex"
                            justifyContent="center"
                            alignItems="center"
                            height="100%"
                        >
                            <Link to="create">
                                <IconButton color="secondary">
                                    <AddCircleIcon sx={{ fontSize: "3em" }} />
                                </IconButton>
                            </Link>
                        </Box>
                    </Grid>
                )}
            </Grid>
        </Box>
    );
};

export default ManufacturesListPage;
