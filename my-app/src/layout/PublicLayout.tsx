import { Outlet, Navigate } from "react-router-dom";
import Navbar from "../components/shared/Navbar";

const PublicLayout = () => {
    const isLoggedIn = !!localStorage.getItem('user');
    if (isLoggedIn) return <Navigate to="/dashboard" replace />;
    return (
        <div className="body">
            <Navbar />
            <Outlet />
        </div>
    );
};

export default PublicLayout;