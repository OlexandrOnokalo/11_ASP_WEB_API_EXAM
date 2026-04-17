import { createContext, useContext, useState } from "react";

export const AuthContext = createContext(null);


export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({children}) => {
    const [isAuth, setIsAuth] = useState(false);
    const [user, setUser] = useState(null);

    function login() {
        setIsAuth(true);
        const auth = localStorage.getItem("auth");
        if(auth) {
            setUser(JSON.parse(auth));
        }
    }

    function logout() {
        setIsAuth(false);
        setUser(null);
        localStorage.removeItem("auth");
    }

    return (
        <AuthContext.Provider value={{ isAuth, login, logout, user }}>
            {children}
        </AuthContext.Provider>
    )
}


const oldState = {
    balance: 0,
    pinCode: 1345,
}

const newState = {...oldState, balance: 10};