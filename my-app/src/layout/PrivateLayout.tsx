import { Outlet, Navigate } from "react-router-dom";
import Navbar from "../components/shared/Navbar";

const PrivateLayout = () => {
    const isLoggedIn = !!localStorage.getItem('user');
    if (!isLoggedIn) return <Navigate to="/user-login" replace />;
    return (
        <div className="body">
            <Navbar />
            <Outlet />
        </div>
    );
};

export default PrivateLayout;