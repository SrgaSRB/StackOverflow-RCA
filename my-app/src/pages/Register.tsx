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
              <div id="w-node-_24812e72-d829-c385-2c75-c2af40124d3d-b7b894d8" className="text-block">Join Stack Overflow</div>
              <div id="w-node-_24812e72-d829-c385-2c75-c2af40124d3f-b7b894d8" className="secondary-color">
                Create your account to start asking and answering questions<br />
              </div>
              <div className="form-group">
                <label htmlFor="firstName" className="form-label">First Name</label>
                <input
                  className="input w-input"
                  name="firstName"
                  type="text"
                  id="firstName"
                  value={formData.firstName}
                  onChange={handleChange}
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="lastName" className="form-label">Last Name</label>
                <input
                  className="input w-input"
                  name="lastName"
                  type="text"
                  id="lastName"
                  value={formData.lastName}
                  onChange={handleChange}
                  required
                />
              </div>
              <div id="w-node-_7947cb76-7d0f-c2d8-cb4d-e2953045e6a3-b7b894d8" className="form-group">
                <label htmlFor="username" className="form-label">Username</label>
                <input
                  className="input w-input"
                  name="username"
                  type="text"
                  id="username"
                  value={formData.username}
                  onChange={handleChange}
                  required
                />
              </div>
              <div id="w-node-_721851b3-075e-e0b7-aad9-9d7fcc88d8d9-b7b894d8" className="form-group">
                <label htmlFor="email" className="form-label">Email</label>
                <input
                  className="input w-input"
                  name="email"
                  type="email"
                  id="email"
                  value={formData.email}
                  onChange={handleChange}
                  required
                />
              </div>
              <div id="w-node-f0f9bd6e-02ef-f4dd-2123-e0724665c7fd-b7b894d8" className="form-group">
                <label htmlFor="gender" className="form-label">Gender</label>
                <select
                  className="input w-select"
                  name="gender"
                  id="gender"
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
                <label htmlFor="country" className="form-label">Country</label>
                <input
                  className="input w-input"
                  name="country"
                  type="text"
                  id="country"
                  value={formData.country}
                  onChange={handleChange}
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="city" className="form-label">City</label>
                <input
                  className="input w-input"
                  name="city"
                  type="text"
                  id="city"
                  value={formData.city}
                  onChange={handleChange}
                  required
                />
              </div>
              <div id="w-node-_2642e2f5-f9cc-c896-d9d5-c3c649ec955c-b7b894d8" className="form-group">
                <label htmlFor="streetAddress" className="form-label">Street address</label>
                <input
                  className="input w-input"
                  name="streetAddress"
                  id="streetAddress"
                  type="text"
                  value={formData.streetAddress}
                  onChange={handleChange}
                  required
                />
              </div>
              <div id="w-node-_69a027f1-4018-67d0-7b0e-084c05eac9e5-b7b894d8" className="form-group">
                <label htmlFor="password"  className="form-label">Password</label>
                <input
                  className="input w-input"
                  name="password"
                  type="password"
                  id="password"
                  value={formData.password}
                  onChange={handleChange}
                  required
                />
              </div>
              <div id="w-node-e1b02c46-bdc8-6422-2293-6fc496137f61-b7b894d8" className="form-group">
                <label htmlFor="confirmPassword"  className="form-label">Repeat Password</label>
                <input
                  className="input w-input"
                  name="confirmPassword"
                  type="password"
                  id="confirmPassword"
                  value={formData.confirmPassword}
                  onChange={handleChange}
                  required
                />
              </div>
              <input
                type="submit"
                id="w-node-_24812e72-d829-c385-2c75-c2af40124d49-b7b894d8"
                className="primary-button w-button"
                value="Create account"
              />
              <div id="w-node-_24812e72-d829-c385-2c75-c2af40124d4a-b7b894d8" className="div-block">
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