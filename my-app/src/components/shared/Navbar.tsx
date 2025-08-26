import { Link } from "react-router-dom";

const Navbar = () => {
    
    return (
    <div className="navbar w-nav">
        <div className="w-layout-blockcontainer container nav-container w-container">
            <div className="navbar-wrapper">
                <Link to="/" >Logo</Link>
                <div className="form-block-2 w-form">
                    <form id="email-form-2" name="email-form-2">
                        <div className="div-block-3">
                            <input className="input search w-input" name="field-2" data-name="Field 2"
                                placeholder="Search" type="text" id="field-2" required />
                            <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78ec7d59d91133ecf299e_magnifying-glass.png"
                                loading="lazy" alt="" className="image-4" />
                        </div>
                    </form>
                </div>
                <div className="navbar-buttons">
                    <Link to="/user-login" className="primary-button secondary--button w-button">Log in</Link>
                    <Link to="/user-register" className="primary-button w-button">Sign up</Link>
                </div>
            </div>
        </div>
    </div>

    );
};

export default Navbar;