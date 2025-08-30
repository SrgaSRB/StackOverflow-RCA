import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

const Login = () => {
  const [formData, setFormData] = useState({
    email: '',
    password: ''
  });
  const navigate = useNavigate();

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
        
        console.log('User received from login:', user);
        
        // Handle both RowKey and rowKey properties
        const userId = user.RowKey || user.rowKey || user.id;
        console.log('User ID:', userId);
        
        // Ensure we have a valid user ID
        if (user && userId) {
          // Normalize the user object to have consistent property names
          const normalizedUser = {
            ...user,
            id: userId,
            RowKey: userId,
            rowKey: userId
          };
          
          localStorage.setItem('user', JSON.stringify(normalizedUser));
          console.log('User saved to localStorage');
          navigate('/dashboard');
        } else {
          console.error('User object missing ID:', user);
          alert('Login failed: User data incomplete');
        }
      } else {
        const errorMsg = await response.text();
        console.error('Login failed:', errorMsg);
        alert(errorMsg || 'Login failed');
      }
    } catch (error) {
      console.error('Login error:', error);
      alert('Login failed - network error');
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