import { Link, useNavigate } from 'react-router-dom';
import { useState, useRef, useEffect } from 'react';

const Navbar = () => {
  const navigate = useNavigate();
  const isLoggedIn = !!localStorage.getItem('user');
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const handleLogout = () => {
    localStorage.removeItem('user');
    navigate('/user-login');
  };

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsDropdownOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  return (
    <div data-animation="default" className="navbar w-nav">
      <div className="w-layout-blockcontainer container nav-container w-container">
        <div className="navbar-wrapper">
          <Link to="/dashboard" className="brand w-nav-brand">
            <div>Logo</div>
          </Link>
          {isLoggedIn && (
            <div className="form-block-2 w-form">
              <form
                id="search-form"
                name="search-form"
                onSubmit={(e) => {
                  e.preventDefault();
                  const search = (e.currentTarget.elements.namedItem('search') as HTMLInputElement).value;
                  navigate(`/dashboard?search=${search}`);
                }}
              >
                <div className="div-block-3">
                  <input
                    className="input search w-input"
                    maxLength={256}
                    name="search"
                    data-name="Search"
                    placeholder="Search"
                    type="text"
                    id="search"
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
            {isLoggedIn && (
              <div ref={dropdownRef} style={{ position: 'relative', marginRight: '10px' }}>
                <button
                  onClick={() => setIsDropdownOpen(!isDropdownOpen)}
                  className="primary-button w-button"
                  style={{
                    backgroundColor: 'white',
                    color: '#38ad73',
                    border: '1px solid #38ad73',
                    display: 'flex',
                    alignItems: 'center',
                  }}
                >
                  Menu
                  <span
                    style={{
                      marginLeft: '5px',
                      transform: isDropdownOpen ? 'rotate(180deg)' : 'rotate(0deg)',
                      transition: 'transform 0.2s',
                    }}
                  >
                    ▼
                  </span>
                </button>
                {isDropdownOpen && (
                  <div
                    style={{
                      position: 'absolute',
                      top: '100%',
                      left: 0,
                      backgroundColor: 'white',
                      border: '1px solid #ccc',
                      borderRadius: '4px',
                      zIndex: 1000,
                      display: 'flex',
                      flexDirection: 'column',
                      width: 'max-content'
                    }}
                  >
                    <Link to="/dashboard" className="primary-button secondary--button w-button" style={{ margin: '5px', textAlign: 'left' }}>
                      Dashboard
                    </Link>
                    <Link to="/create-post" className="primary-button secondary--button w-button" style={{ margin: '5px', textAlign: 'left' }}>
                      Create Post
                    </Link>
                    <Link to="/profile" className="primary-button secondary--button w-button" style={{ margin: '5px', textAlign: 'left' }}>
                      Profile
                    </Link>
                  </div>
                )}
              </div>
            )}
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