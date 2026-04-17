import { useRef, useState } from "react";
import { useAuth } from "../../../context/AuthContext";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Checkbox from "@mui/material/Checkbox";
import CssBaseline from "@mui/material/CssBaseline";
import FormControlLabel from "@mui/material/FormControlLabel";
import Divider from "@mui/material/Divider";
import FormLabel from "@mui/material/FormLabel";
import FormControl from "@mui/material/FormControl";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import Stack from "@mui/material/Stack";
import MuiCard from "@mui/material/Card";
import { styled } from "@mui/material/styles";
import ForgotPassword from "./../components/ForgotPassword";
import { Link, useNavigate } from "react-router-dom";
import { GoogleIcon, FacecarIcon } from "./../components/CustomIcons";

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

const LoginPage = () => {
    const [open, setOpen] = useState(false);
    const [errors, setErrors] = useState({});
    const emailRef = useRef(null);
    const passwordRef = useRef(null);
    const navigate = useNavigate();
    const { login } = useAuth();

    const handleClickOpen = () => {
        setOpen(true);
    };

    const handleClose = () => {
        setOpen(false);
    };

    const handleSubmit = (event) => {
        event.preventDefault();

        const cred = {
            email: emailRef.current.value,
            password: passwordRef.current.value,
        };

        const result = validate(cred);
        if (!result.result) {
            setErrors(result.errors);
            return;
        } else {
            setErrors({});
        }

        const localData = localStorage.getItem("users");
        if(!localData) {
            navigate("/register");
        }
        const users = JSON.parse(localData);

        const user = users.find(u => u.email === cred.email);

        if(!user || cred.password !== user.password) {
            alert("Пошта або пароль вказані невірно");
            return;
        }

        localStorage.setItem("auth", JSON.stringify(user));
        login();
        navigate("/", { replace: true });
    };

    function validate(formValues) {
        const validateErrors = {};
        let result = true;
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (formValues.email.length === 0) {
            validateErrors.email = "Обов'язкове поле";
            result = false;
        } else if (!emailRegex.test(formValues.email)) {
            validateErrors.email = "Невірний формат пошти";
            result = false;
        }

        if (formValues.password.length === 0) {
            validateErrors.password = "Обов'язкове поле";
            result = false;
        } else if (formValues.password.length < 6) {
            validateErrors.password = "Мінімальна довжина 6 символів";
            result = false;
        }

        return { result: result, errors: validateErrors };
    }

    const getError = (prop) => {
        return errors[prop] ? (
            <Typography sx={{ mx: 1, color: "red" }} variant="h7">
                {errors[prop]}
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
                        Sign in
                    </Typography>
                    <Box
                        component="form"
                        onSubmit={handleSubmit}
                        noValidate
                        sx={{
                            display: "flex",
                            flexDirection: "column",
                            width: "100%",
                            gap: 2,
                        }}
                    >
                        <FormControl>
                            <FormLabel htmlFor="email">Email</FormLabel>
                            <TextField
                                inputRef={emailRef}
                                type="email"
                                name="email"
                                placeholder="your@email.com"
                                autoComplete="email"
                                autoFocus
                                required
                                fullWidth
                                variant="outlined"
                            />
                            {getError("email")}
                        </FormControl>
                        <FormControl>
                            <FormLabel htmlFor="password">Password</FormLabel>
                            <TextField
                                inputRef={passwordRef}
                                name="password"
                                placeholder="••••••"
                                type="password"
                                autoComplete="current-password"
                                fullWidth
                                variant="outlined"
                            />
                            {getError("password")}
                        </FormControl>
                        <FormControlLabel
                            control={
                                <Checkbox value="remember" color="primary" />
                            }
                            label="Remember me"
                        />
                        <ForgotPassword open={open} handleClose={handleClose} />
                        <Button type="submit" fullWidth variant="contained">
                            Sign in
                        </Button>
                        <Link
                            component="button"
                            type="button"
                            onClick={handleClickOpen}
                            variant="body2"
                            sx={{ alignSelf: "center" }}
                        >
                            Forgot your password?
                        </Link>
                    </Box>
                    <Divider>or</Divider>
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
                            Ще не зареєстровані?{" "}
                            <Link
                                to="/register"
                                style={{ alignSelf: "center" }}
                            >
                                Зареєструватися
                            </Link>
                        </Typography>
                    </Box>
                </Card>
            </SignInContainer>
        </>
    );
};

export default LoginPage;
