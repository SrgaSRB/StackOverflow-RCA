import { useState } from 'react';
import { Link } from 'react-router-dom';
import './style.css';

const Login = () => {
  const [formData, setFormData] = useState({
    email: '',
    password: ''
  });

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    // handle login logic
  };

  return (
    <section className="login-section">
      <div className="container">
        <div className="login-wrapper">
          <div className="form-block">
            <form className="form" onSubmit={handleSubmit}>
              <div className="text-block">Sign in to Stack Overflow</div>
              <div className="secondary-color">
                Enter your email and password to access your account
              </div>
              <div className="form-group">
                <label htmlFor="email" className="form-label">Email</label>
                <input
                  className="input"
                  type="email"
                  id="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="password" className="form-label">Password</label>
                <input
                  className="input"
                  type="password"
                  id="password"
                  name="password"
                  value={formData.password}
                  onChange={handleChange}
                  required
                />
              </div>
              <button type="submit" className="primary-button">Submit</button>
              <div className="div-block">
                <div className="secondary-color">Don't have an account?</div>
                <Link className="link" to="/user-register">Sign up</Link>
              </div>
            </form>
          </div>
        </div>
      </div>
    </section>
  );
};

export default Login;