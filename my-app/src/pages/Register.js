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

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    console.log('Registration data:', formData);
  };

  return (
    <section className="register-section">
      <div className="w-layout-blockcontainer container w-container">
        <div className="register-wrapper">
          <div className="form-block w-form">
            <div id="email-form" className="form register-form">
              <div id="w-node-_24812e72-d829-c385-2c75-c2af40124d3d-b7b894d8" className="text-block">
                Join Stack Overflow
              </div>
              <div id="w-node-_24812e72-d829-c385-2c75-c2af40124d3f-b7b894d8" className="secondary-color">
                Create your account to start asking and answering questions<br />
              </div>
              
              <div className="form-group">
                <label htmlFor="firstName" className="form-label">First Name</label>
                <input
                  className="input w-input"
                  maxLength="256"
                  name="firstName"
                  data-name="Name"
                  placeholder=""
                  type="text"
                  id="firstName"
                  value={formData.firstName}
                  onChange={handleChange}
                />
              </div>
              
              <div className="form-group">
                <label htmlFor="lastName" className="form-label">Last Name</label>
                <input
                  className="input w-input"
                  maxLength="256"
                  name="lastName"
                  data-name="Name"
                  placeholder=""
                  type="text"
                  id="lastName"
                  value={formData.lastName}
                  onChange={handleChange}
                />
              </div>
              
              <div id="w-node-_7947cb76-7d0f-c2d8-cb4d-e2953045e6a3-b7b894d8" className="form-group">
                <label htmlFor="username" className="form-label">Username</label>
                <input
                  className="input w-input"
                  maxLength="256"
                  name="username"
                  data-name="Name"
                  placeholder=""
                  type="text"
                  id="username"
                  value={formData.username}
                  onChange={handleChange}
                />
              </div>
              
              <div id="w-node-_721851b3-075e-e0b7-aad9-9d7fcc88d8d9-b7b894d8" className="form-group">
                <label htmlFor="email" className="form-label">Email</label>
                <input
                  className="input w-input"
                  maxLength="256"
                  name="email"
                  data-name="Name"
                  placeholder=""
                  type="text"
                  id="email"
                  value={formData.email}
                  onChange={handleChange}
                />
              </div>
              
              <div id="w-node-f0f9bd6e-02ef-f4dd-2123-e0724665c7fd-b7b894d8" className="form-group">
                <label htmlFor="gender" className="form-label">Gender</label>
                <select
                  id="gender"
                  name="gender"
                  data-name="Field"
                  className="input w-select"
                  value={formData.gender}
                  onChange={handleChange}
                >
                  <option value="">Select one...</option>
                  <option value="First">First choice</option>
                  <option value="Second">Second choice</option>
                  <option value="Third">Third choice</option>
                </select>
              </div>
              
              <div className="form-group">
                <label htmlFor="country" className="form-label">Country</label>
                <input
                  className="input w-input"
                  maxLength="256"
                  name="country"
                  data-name="Name"
                  placeholder=""
                  type="text"
                  id="country"
                  value={formData.country}
                  onChange={handleChange}
                />
              </div>
              
              <div className="form-group">
                <label htmlFor="city" className="form-label">City</label>
                <input
                  className="input w-input"
                  maxLength="256"
                  name="city"
                  data-name="Name"
                  placeholder=""
                  type="text"
                  id="city"
                  value={formData.city}
                  onChange={handleChange}
                />
              </div>
              
              <div id="w-node-_2642e2f5-f9cc-c896-d9d5-c3c649ec955c-b7b894d8" className="form-group">
                <label htmlFor="streetAddress" className="form-label">Street address</label>
                <input
                  className="input w-input"
                  maxLength="256"
                  name="streetAddress"
                  data-name="Name"
                  placeholder=""
                  type="text"
                  id="streetAddress"
                  value={formData.streetAddress}
                  onChange={handleChange}
                />
              </div>
              
              <div id="w-node-_69a027f1-4018-67d0-7b0e-084c05eac9e5-b7b894d8" className="form-group">
                <label htmlFor="password" className="form-label">Password</label>
                <input
                  className="input w-input"
                  maxLength="256"
                  name="password"
                  data-name="Name"
                  placeholder=""
                  type="password"
                  id="password"
                  value={formData.password}
                  onChange={handleChange}
                />
              </div>
              
              <div id="w-node-e1b02c46-bdc8-6422-2293-6fc496137f61-b7b894d8" className="form-group">
                <label htmlFor="confirmPassword" className="form-label">Repeat Password</label>
                <input
                  className="input w-input"
                  maxLength="256"
                  name="confirmPassword"
                  data-name="Name"
                  placeholder=""
                  type="password"
                  id="confirmPassword"
                  value={formData.confirmPassword}
                  onChange={handleChange}
                />
              </div>
              
              <button
                type="button"
                data-wait="Please wait..."
                id="w-node-_24812e72-d829-c385-2c75-c2af40124d49-b7b894d8"
                className="primary-button w-button"
                onClick={handleSubmit}
              >
                Create account
              </button>
              
              <div id="w-node-_24812e72-d829-c385-2c75-c2af40124d4a-b7b894d8" className="div-block">
                <div className="secondary-color">Already have an account?</div>
                <a className="link" href="/user-login">Sign in</a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default Register;