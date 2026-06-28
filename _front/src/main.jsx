import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import "./index.css";
import App from "./App.jsx";
import { AuthProvider } from "./context/AuthContext.jsx";
import { store } from "./store/store.js";
import { Provider } from "react-redux";

// Порядок обгорток важливий:
// Provider — найзовніші, щоб store був доступний скрізь включно з AuthProvider
// BrowserRouter — до AuthProvider, бо useNavigate потребує роутер-контексту
// AuthProvider — до App, бо App одразу читає useAuth()
createRoot(document.getElementById("root")).render(
    <Provider store={store}>
        {/* future-прапори гасять deprecation-попередження при переході на Router v7 */}
        <BrowserRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
            <AuthProvider>
                <App />
            </AuthProvider>
        </BrowserRouter>
    </Provider>,
);
