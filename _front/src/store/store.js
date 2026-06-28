import { configureStore } from "@reduxjs/toolkit";
import { rootReducer } from "./reducers/rootReducer";

// configureStore з RTK автоматично підключає Redux DevTools в dev-режимі
// і додає thunk middleware — більше нічого налаштовувати не треба
export const store = configureStore({
    reducer: rootReducer
})