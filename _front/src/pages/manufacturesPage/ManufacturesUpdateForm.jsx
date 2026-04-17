import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import FormLabel from "@mui/material/FormLabel";
import FormControl from "@mui/material/FormControl";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import Stack from "@mui/material/Stack";
import MuiCard from "@mui/material/Card";
import { styled } from "@mui/material/styles";
import { useNavigate, useParams } from "react-router-dom";
import { useFormik } from "formik";
import { object, string } from "yup";
import { useEffect } from "react";
import axios from "axios";
import { useDispatch, useSelector } from "react-redux";

const Card = styled(MuiCard)(({ theme }) => ({
    display: "flex",
    flexDirection: "column",
    alignSelf: "center",
    width: "100%",
    padding: theme.spacing(4),
    gap: theme.spacing(2),
    margin: "0px auto",
    [theme.breakpoints.up("sm")]: {
        maxWidth: "450px",
    },
    boxShadow:
        "hsla(220, 30%, 5%, 0.05) 0px 5px 15px 0px, hsla(220, 25%, 10%, 0.05) 0px 15px 35px -5px",
    ...theme.applyStyles("dark", {
        boxShadow:
            "hsla(220, 30%, 5%, 0.5) 0px 5px 15px 0px, hsla(220, 25%, 10%, 0.08) 0px 15px 35px -5px",
    }),
}));

const SignInContainer = styled(Stack)(({ theme }) => ({
    minHeight: "calc((1 - var(--template-frame-height, 0)) * 100dvh)",
    padding: theme.spacing(2),
    [theme.breakpoints.up("sm")]: {
        padding: theme.spacing(4),
    },
    "&::before": {
        content: '""',
        display: "block",
        position: "absolute",
        zIndex: -1,
        inset: 0,
        backgroundImage:
            "radial-gradient(ellipse at 50% 50%, hsl(210, 100%, 97%), hsl(0, 0%, 100%))",
        backgroundRepeat: "no-repeat",
        ...theme.applyStyles("dark", {
            backgroundImage:
                "radial-gradient(at 50% 50%, hsla(210, 100%, 16%, 0.5), hsl(220, 30%, 5%))",
        }),
    },
}));

const initValues = {
    id: "",
    name: "",
};

const ManufacturesUpdateForm = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const { id } = useParams();
    const { Manufactures } = useSelector((state) => state.manufacture);

    useEffect(() => {
        const manufacture = Manufactures.find(a => a.id == id);
        if (manufacture) {
            formik.setValues({
                id: manufacture.id,
                name: manufacture.name,
            });
        }
    }, [Manufactures, id]);

    async function handleSubmit(newmanufacture) {
        const ManufacturesUrl = import.meta.env.VITE_MANUFACTURES_URL;
        const response = await axios.put(`${ManufacturesUrl}/${newmanufacture.id}`, {
            id: newmanufacture.id,
            name: newmanufacture.name,
        });
        if (response.status === 200) {
            const updated = response.data && response.data.data ? response.data.data : response.data;
            const index = Manufactures.findIndex(a => a.id == newmanufacture.id);
            let newManufactures = [...Manufactures];
            if (index !== -1) newManufactures[index] = updated;
            dispatch({ type: "updatemanufacture", payload: newManufactures });
            navigate("/Manufactures");
        }
    }

    const getError = (prop) => {
        return formik.touched[prop] && formik.errors[prop] ? (
            <Typography sx={{ mx: 1, color: "red" }} variant="h7">
                {formik.errors[prop]}
            </Typography>
        ) : null;
    };

    const validationScheme = object({
        name: string()
            .required("Обов'язкове поле")
            .max(100, "Максимальна довжина 100 символів"),
    });

    const formik = useFormik({
        initialValues: initValues,
        onSubmit: handleSubmit,
        validationSchema: validationScheme,
    });

    return (
        <Box>
            <SignInContainer direction="column" justifyContent="space-between">
                <Card variant="outlined">
                    <Typography
                        component="h1"
                        variant="h4"
                        sx={{
                            width: "100%",
                            fontSize: "clamp(2rem, 10vw, 2.15rem)",
                        }}
                    >
                Редагувати виробника
                    </Typography>
                    <Box
                        component="form"
                        onSubmit={formik.handleSubmit}
                        sx={{
                            display: "flex",
                            flexDirection: "column",
                            width: "100%",
                            gap: 2,
                        }}
                    >
                        <FormControl>
                            <FormLabel htmlFor="name">Назва виробника</FormLabel>
                            <TextField
                                id="name"
                                type="text"
                                name="name"
                                placeholder="Введіть назву виробника"
                                autoComplete="name"
                                autoFocus
                                required
                                fullWidth
                                variant="outlined"
                                value={formik.values.name}
                                onChange={formik.handleChange}
                                onBlur={formik.handleBlur}
                            />
                            {getError("name")}
                        </FormControl>
                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            disabled={!formik.isValid || !formik.dirty}
                        >
                            Оновити
                        </Button>
                    </Box>
                </Card>
            </SignInContainer>
        </Box>
    );
};

export default ManufacturesUpdateForm;