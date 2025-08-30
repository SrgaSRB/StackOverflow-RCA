import { Link, useNavigate } from 'react-router-dom';

const Navbar = () => {
  const navigate = useNavigate();
  const isLoggedIn = !!localStorage.getItem('user');

  const handleLogout = () => {
    localStorage.removeItem('user');
    navigate('/user-login');
  };

  return (
    <div data-animation="default" className="navbar w-nav">
      <div className="w-layout-blockcontainer container nav-container w-container">
        <div className="navbar-wrapper">
          <div>Logo</div>
          {isLoggedIn && (
            <div className="form-block-2 w-form">
              <form id="search-form" name="search-form">
                <div className="div-block-3">
                  <input
                    className="input search w-input"
                    maxLength={256}
                    name="search"
                    data-name="Search"
                    placeholder="Search"
                    type="text"
                    id="search"
                    required
                  />
                  <img
                    src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78ec7d59d91133ecf299e_magnifying-glass.png"
                    loading="lazy"
                    alt=""
                    className="image-4"
                  />
                </div>
              </form>
            </div>
          )}
          <div className="navbar-buttons">
            {isLoggedIn ? (
              <button
                className="primary-button w-button"
                onClick={handleLogout}
                style={{ cursor: 'pointer' }}
              >
                Logout
              </button>
            ) : (
              <>
                <Link
                  to="/user-login"
                  className="primary-button secondary--button w-button"
                >
                  Log in
                </Link>
                <Link
                  to="/user-register"
                  className="primary-button w-button"
                >
                  Sign up
                </Link>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Navbar;