import { Outlet } from "react-router-dom";
import Navbar from "../components/shared/Navbar";

const PrivateLayout = () => {
    return (
        <div className="body">
            <Navbar />
            <Outlet />
        </div>
    );
};

export default PrivateLayout;
