import { useState } from 'react';
import { Link } from 'react-router-dom';

const Login = () => {
  const [formData, setFormData] = useState({
    email: '',
    password: ''
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
  e.preventDefault();
  try {
    const response = await fetch('http://localhost:5167/api/users/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(formData)
    });
    if (response.ok) {
      const user = await response.json();
      localStorage.setItem('user', JSON.stringify(user));
      window.location.href = '/dashboard';
    } else {
      const errorMsg = await response.text();
      alert(errorMsg || 'Login failed');
    }
  } catch (error) {
    alert('Login failed');
  }
};

  return (
    <section className="login-section">
      <div className="w-layout-blockcontainer container w-container">
        <div className="login-wrapper">
          <div className="form-block w-form">
            <form id="login-form" className="form" onSubmit={handleSubmit}>
              <div className="text-block">Sign in to Stack Overflow</div>
              <div className="secondary-color">Enter your email and password to access your account</div>
              <div className="form-group">
                <label className="form-label">Email</label>
                <input
                  className="input w-input"
                  name="email"
                  type="email"
                  value={formData.email}
                  onChange={handleChange}
                  required
                />
              </div>
              <div className="form-group">
                <label className="form-label">Password</label>
                <input
                  className="input w-input"
                  name="password"
                  type="password"
                  value={formData.password}
                  onChange={handleChange}
                  required
                />
              </div>
              <input type="submit" className="primary-button w-button" value="Submit" />
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