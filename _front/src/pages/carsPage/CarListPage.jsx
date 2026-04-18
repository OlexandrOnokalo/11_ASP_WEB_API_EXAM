import { useState, useEffect } from "react";
import { useLocation } from "react-router-dom";
import CarCard from "./CarCard";
import { Box, Grid, CircularProgress, TextField, MenuItem, Button } from "@mui/material";
import ClearIcon from "@mui/icons-material/Clear";
import SearchIcon from "@mui/icons-material/Search";
import { Link } from "react-router-dom";
import { api } from "../../api";
import { useAuth } from "../../context/AuthContext";
import { useDispatch, useSelector } from "react-redux";
import { getItems } from "../../services/responseParsers";

const CARS_ENDPOINT = "cars";
const MANUFACTURES_ENDPOINT = "manufactures";

const CarListPage = () => {
    const dispatch = useDispatch();
    const location = useLocation();
    const { isAdmin } = useAuth();
    const { cars, isLoaded } = useSelector((state) => state.car);
    const [loading, setLoading] = useState(false);
    const [filters, setFilters] = useState({ page:1, page_size:100 });
    const [searchInputs, setSearchInputs] = useState({ name: '', manufactureId: '', year: '', color: '', volume: '', minValue: '', maxValue: '' });
    const [manufactures, setManufactures] = useState([]);

    useEffect(() => {
        const params = new URLSearchParams(location.search);
        const allowed = ["manufactureId", "year", "color", "volume", "minValue", "maxValue", "name", "page_size", "page"];
        const parsed = { page:1, page_size:100 };
        const inputs = { name: '', manufactureId: '', year: '', color: '', volume: '', minValue: '', maxValue: '' };
        
        for (const key of allowed) {
            const v = params.get(key);
            if (v !== null && v !== "") {
                if (key === 'page' || key === 'page_size' || key === 'manufactureId' || key === 'minValue' || key === 'maxValue') {
                    parsed[key] = Number(v);
                    inputs[key] = v;
                } else {
                    parsed[key] = v;
                    inputs[key] = v;
                }
            }
        }
        setFilters(parsed);
        setSearchInputs(inputs);
    }, [location.search]);

    useEffect(()=> {
        const fetch = async () => {
            setLoading(true);
            try {
                let res;
                if (filters.minValue || filters.maxValue) {
                    const params = { page: filters.page || 1, page_size: filters.page_size || 100 };
                    if (filters.minValue) params.minValue = filters.minValue;
                    if (filters.maxValue) params.maxValue = filters.maxValue;
                    res = await api.get(`${CARS_ENDPOINT}/by-price`, { params });
                } else if (filters.name || filters.manufactureId || filters.year || filters.color || filters.volume) {
                    const priority = ['name','manufactureId','year','color','volume'];
                    let prop = priority.find(p => filters[p]);
                    const params = { page: filters.page || 1, page_size: filters.page_size || 100 };
                    if (prop) {
                        if (prop === 'manufactureId') {
                            const manu = manufactures.find(m => String(m.id) === String(filters.manufactureId));
                            if (manu) {
                                params.property = 'manufacture';
                                params.value = manu.name;
                            }
                        } else {
                            params.property = prop;
                            params.value = String(filters[prop]);
                        }
                    }
                    res = await api.get(CARS_ENDPOINT, { params });
                } else {
                    res = await api.get(CARS_ENDPOINT, { params: { page: filters.page || 1, page_size: filters.page_size || 100 } });
                }

                dispatch({ type: 'loadcars', payload: getItems(res) });
            } catch(e){
                console.error(e);
                dispatch({ type: 'loadcars', payload: [] });
            } finally { setLoading(false) }
        };
        fetch();
    }, [dispatch, filters]);

    useEffect(()=> {
        const fetchM = async () => {
            try {
                const res = await api.get(MANUFACTURES_ENDPOINT, { params: { page:1, page_size:100 }});
                setManufactures(getItems(res));
            } catch(e){ console.error(e) }
        };
        fetchM();
    }, []);

    const handleManufactureChange = (e) => {
        const value = e.target.value;
        setSearchInputs(s=>({...s, manufactureId:value}));
        
        if (value) {
            setFilters({ page: 1, page_size: 100, manufactureId: Number(value) });
        } else {
            setFilters({ page: 1, page_size: 100 });
        }
    };

    const handleSearch = () => {
        const newFilters = { page: 1, page_size: 100 };
        if (searchInputs.name?.trim()) newFilters.name = searchInputs.name.trim();
        if (searchInputs.manufactureId) newFilters.manufactureId = Number(searchInputs.manufactureId);
        if (searchInputs.year?.trim()) newFilters.year = searchInputs.year.trim();
        if (searchInputs.color?.trim()) newFilters.color = searchInputs.color.trim();
        if (searchInputs.volume?.trim()) newFilters.volume = searchInputs.volume.trim();
        if (searchInputs.minValue?.trim()) newFilters.minValue = Number(searchInputs.minValue);
        if (searchInputs.maxValue?.trim()) newFilters.maxValue = Number(searchInputs.maxValue);
        setFilters(newFilters);
    };

    const handleClear = () => {
        setSearchInputs({ name: '', manufactureId: '', year: '', color: '', volume: '', minValue: '', maxValue: '' });
        setFilters({ page: 1, page_size: 100 });
    };

    const onDelete = async (id) => {
        if(!confirm('Видалити автомобіль?')) return;
        try{
            await api.delete(`${CARS_ENDPOINT}/${id}`);
            dispatch({ type: 'deletecar', payload: id });
        } catch(e){ console.error(e) }
    };

    return (
        <Box sx={{ pt: 2 }}>
            <Box sx={{ display:'flex', gap:1, mb:3, alignItems:'flex-end', flexWrap: 'wrap' }}>
                <TextField label='По назві' size='small' value={searchInputs.name} onChange={e=> setSearchInputs(s=>({...s, name:e.target.value}))} />
                <TextField select label='Виробник' size='small' value={searchInputs.manufactureId} onChange={handleManufactureChange}>
                    <MenuItem value=''>Усі</MenuItem>
                    {manufactures.map(m=> <MenuItem key={m.id} value={String(m.id)}>{m.name}</MenuItem>)}
                </TextField>
                <TextField label='Рік' size='small' value={searchInputs.year} onChange={e=> setSearchInputs(s=>({...s, year:e.target.value}))} />
                <TextField label='Колір' size='small' value={searchInputs.color} onChange={e=> setSearchInputs(s=>({...s, color:e.target.value}))} />
                <TextField label='Обєм' size='small' value={searchInputs.volume} onChange={e=> setSearchInputs(s=>({...s, volume:e.target.value}))} />
                <TextField label='Мін. ціна' size='small' type='number' value={searchInputs.minValue} onChange={e=> setSearchInputs(s=>({...s, minValue:e.target.value}))} />
                <TextField label='Макс. ціна' size='small' type='number' value={searchInputs.maxValue} onChange={e=> setSearchInputs(s=>({...s, maxValue:e.target.value}))} />
                <Button variant='contained' startIcon={<SearchIcon />} onClick={handleSearch}>Пошук</Button>
                <Button variant='outlined' startIcon={<ClearIcon />} onClick={handleClear}>Очистити</Button>
                {isAdmin && <Button variant='contained' color='success' component={Link} to='create'>Додати</Button>}
            </Box>

            <Grid container spacing={2}>
                {loading && <Grid item xs={12}><CircularProgress /></Grid>}
                {!loading && cars && cars.length === 0 && <Grid item xs={12}>Немає автомобілів</Grid>}
                {!loading && cars && cars.map(car => (
                    <Grid item key={car.id} xs={12} md={6} lg={4}>
                        <CarCard car={car} onDelete={onDelete} canManage={isAdmin} />
                    </Grid>
                ))}
            </Grid>
        </Box>
    );
};

export default CarListPage;
