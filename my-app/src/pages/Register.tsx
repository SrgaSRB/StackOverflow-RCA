import { useState } from 'react';

const Register = () => {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    username: '',
    email: '',
    gender: '',
    country: '',
    city: '',
    streetAddress: '',
    password: '',
    confirmPassword: ''
  });

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (formData.password !== formData.confirmPassword) {
      alert('Passwords do not match!');
      return;
    }
    // Pripremi payload bez confirmPassword
    const { confirmPassword, ...payload } = formData;

    try {
      const response = await fetch('http://localhost:5167/api/users/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      if (response.ok) {
        const user = await response.json();
        const userId = user.RowKey || user.rowKey || user.id;
        if (user && userId) {
          const normalizedUser = {
            ...user,
            id: userId,
            RowKey: userId,
            rowKey: userId
          };
          localStorage.setItem('user', JSON.stringify(normalizedUser));
          alert('Registration successful!');
        } else {
          alert('Registration failed: User data incomplete');
        }
        setFormData({
          firstName: '',
          lastName: '',
          username: '',
          email: '',
          gender: '',
          country: '',
          city: '',
          streetAddress: '',
          password: '',
          confirmPassword: ''
        });
      } else {
        const errorMsg = await response.text();
        alert(errorMsg || 'Registration failed');
      }
    } catch (error) {
      alert('Registration failed');
    }
  };

  return (
    <section className="register-section">
      <div className="w-layout-blockcontainer container w-container">
        <div className="register-wrapper">
          <div className="form-block w-form">
            <form id="email-form" className="form register-form" onSubmit={handleSubmit}>
              <div className="text-block">Join Stack Overflow</div>
              <div className="secondary-color">
                Create your account to start asking and answering questions<br />
              </div>
              <div className="form-group">
                <label className="form-label">First Name</label>
                <input
                  className="input w-input"
                  name="firstName"
                  type="text"
                  value={formData.firstName}
                  onChange={handleChange}
                  required
                />
              </div>
              <div className="form-group">
                <label className="form-label">Last Name</label>
                <input
                  className="input w-input"
                  name="lastName"
                  type="text"
                  value={formData.lastName}
                  onChange={handleChange}
                  required
                />
              </div>
              <div className="form-group">
                <label className="form-label">Username</label>
                <input
                  className="input w-input"
                  name="username"
                  type="text"
                  value={formData.username}
                  onChange={handleChange}
                  required
                />
              </div>
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
                <label className="form-label">Gender</label>
                <select
                  className="input w-select"
                  name="gender"
                  value={formData.gender}
                  onChange={handleChange}
                  required
                >
                  <option value="">Select one...</option>
                  <option value="male">Male</option>
                  <option value="female">Female</option>
                  <option value="other">Other</option>
                </select>
              </div>
              <div className="form-group">
                <label className="form-label">Country</label>
                <input
                  className="input w-input"
                  name="country"
                  type="text"
                  value={formData.country}
                  onChange={handleChange}
                  required
                />
              </div>
              <div className="form-group">
                <label className="form-label">City</label>
                <input
                  className="input w-input"
                  name="city"
                  type="text"
                  value={formData.city}
                  onChange={handleChange}
                  required
                />
              </div>
              <div className="form-group">
                <label className="form-label">Street address</label>
                <input
                  className="input w-input"
                  name="streetAddress"
                  type="text"
                  value={formData.streetAddress}
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
              <div className="form-group">
                <label className="form-label">Repeat Password</label>
                <input
                  className="input w-input"
                  name="confirmPassword"
                  type="password"
                  value={formData.confirmPassword}
                  onChange={handleChange}
                  required
                />
              </div>
              <input
                type="submit"
                className="primary-button w-button"
                value="Create account"
              />
              <div className="div-block">
                <div className="secondary-color">Already have an account?</div>
                <a className="link" href="/user-login">Sign in</a>
              </div>
            </form>
          </div>
        </div>
      </div>
    </section>
  );
};

export default Register;