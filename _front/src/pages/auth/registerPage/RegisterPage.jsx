import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import CssBaseline from "@mui/material/CssBaseline";
import Divider from "@mui/material/Divider";
import FormLabel from "@mui/material/FormLabel";
import FormControl from "@mui/material/FormControl";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import Stack from "@mui/material/Stack";
import MuiCard from "@mui/material/Card";
import { styled } from "@mui/material/styles";
import { Link, useNavigate } from "react-router-dom";
import { GoogleIcon, FacecarIcon } from "./../components/CustomIcons";
import { object, string, ref } from "yup";
import { useFormik } from "formik";

const Card = styled(MuiCard)(({ theme }) => ({
    display: "flex",
    flexDirection: "column",
    alignSelf: "center",
    width: "100%",
    padding: theme.spacing(4),
    gap: theme.spacing(2),
    margin: "auto",
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

const RegisterPage = () => {
    const navigate = useNavigate();

    const handleSubmit = (userData) => {
        let users = [];
        const localData = localStorage.getItem("users");
        if(localData) {
            users = JSON.parse(localData);
        }

        if(users.length === 0) {
            userData.role = "admin"
        } else {
            userData.role = "user"
        }

        const index = users.findIndex(u => u.email === userData.email);

        if(index !== -1) {
            alert(`Користувач з поштою '${userData.email}' вже існує`);
            return;
        }

        delete userData.confirmPassword;
        users.push(userData);
        localStorage.setItem("users", JSON.stringify(users));
        navigate("/login");
    };

    const validationSchema = object({
        firstName: string().required("Обов'язкове поле"),
        lastName: string().required("Обов'язкове поле"),
        email: string()
            .required("Обов'язкове поле")
            .email("Невірний формат пошти"),
        password: string()
            .required("Обов'язкове поле")
            .min(6, "Мінімальна довжина 6 символів"),
        confirmPassword: string()
            .required("Обов'язкове поле")
            .min(6, "Мінімальна довжина 6 символів")
            .oneOf([ref("password"), null], "Паролі повинні збігатися"),
    });

    const initValues = {
        firstName: "",
        lastName: "",
        email: "",
        password: "",
        confirmPassword: "",
    };

    const formik = useFormik({
        initialValues: initValues,
        onSubmit: handleSubmit,
        validationSchema: validationSchema,
    });

    const getError = (prop) => {
        return formik.errors[prop] ? (
            <Typography sx={{ mx: 1, color: "red" }} variant="h7">
                {formik.errors[prop]}
            </Typography>
        ) : null;
    };

    return (
        <>
            <CssBaseline enableColorScheme />
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
                        Реєстрація
                    </Typography>
                    <Box
                        component="form"
                        onSubmit={formik.handleSubmit}
                        noValidate
                        sx={{
                            display: "flex",
                            flexDirection: "column",
                            width: "100%",
                            gap: 2,
                        }}
                    >
                        <FormControl>
                            <FormLabel htmlFor="firstName">Ім'я</FormLabel>
                            <TextField
                                type="text"
                                name="firstName"
                                placeholder="Джон"
                                autoComplete="firstName"
                                autoFocus
                                fullWidth
                                variant="outlined"
                                value={formik.values.firstName}
                                onChange={formik.handleChange}
                                onBlur={formik.handleBlur}
                            />
                            {getError("firstName")}
                        </FormControl>
                        <FormControl>
                            <FormLabel htmlFor="lastName">Прізвище</FormLabel>
                            <TextField
                                type="text"
                                name="lastName"
                                placeholder="Сміт"
                                autoComplete="lastName"
                                fullWidth
                                variant="outlined"
                                value={formik.values.lastName}
                                onChange={formik.handleChange}
                                onBlur={formik.handleBlur}
                            />
                            {getError("lastName")}
                        </FormControl>
                        <FormControl>
                            <FormLabel htmlFor="email">Пошта</FormLabel>
                            <TextField
                                type="email"
                                name="email"
                                placeholder="your@email.com"
                                autoComplete="email"
                                fullWidth
                                variant="outlined"
                                value={formik.values.email}
                                onChange={formik.handleChange}
                                onBlur={formik.handleBlur}
                            />
                            {getError("email")}
                        </FormControl>
                        <FormControl>
                            <FormLabel htmlFor="password">Пароль</FormLabel>
                            <TextField
                                name="password"
                                placeholder="••••••"
                                type="password"
                                autoComplete="current-password"
                                fullWidth
                                variant="outlined"
                                value={formik.values.password}
                                onChange={formik.handleChange}
                                onBlur={formik.handleBlur}
                            />
                            {getError("password")}
                        </FormControl>
                        <FormControl>
                            <FormLabel htmlFor="confirmPassword">
                                Підтвердити пароль
                            </FormLabel>
                            <TextField
                                name="confirmPassword"
                                placeholder="••••••"
                                type="password"
                                fullWidth
                                variant="outlined"
                                value={formik.values.confirmPassword}
                                onChange={formik.handleChange}
                                onBlur={formik.handleBlur}
                            />
                            {getError("confirmPassword")}
                        </FormControl>
                        <Button type="submit" fullWidth variant="contained">
                            Зареєструватися
                        </Button>
                    </Box>
                    <Divider>або</Divider>
                    <Box
                        sx={{
                            display: "flex",
                            flexDirection: "column",
                            gap: 2,
                        }}
                    >
                        <Button
                            fullWidth
                            variant="outlined"
                            onClick={() => alert("Sign in with Google")}
                            startIcon={<GoogleIcon />}
                        >
                            Sign in with Google
                        </Button>
                        <Button
                            fullWidth
                            variant="outlined"
                            onClick={() => alert("Sign in with Facecar")}
                            startIcon={<FacecarIcon />}
                        >
                            Sign in with Facecar
                        </Button>
                        <Typography sx={{ textAlign: "center" }}>
                            Зареєстровані?{" "}
                            <Link to="/login" style={{ alignSelf: "center" }}>
                                Увійти
                            </Link>
                        </Typography>
                    </Box>
                </Card>
            </SignInContainer>
        </>
    );
};

export default RegisterPage;
