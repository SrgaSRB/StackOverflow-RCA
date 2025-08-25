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

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    // handle login logic
  };

  return (
    <section className="login-section">
        <div className="w-layout-blockcontainer container w-container">
            <div className="login-wrapper">
                <div className="form-block w-form">
                    <form id="email-form" className="form">
                        <div className="text-block">Sign in to Stack Overflow</div>
                        <div className="secondary-color">Enter your email and password to access your account</div>
                        <div className="form-group">
                            <label className="form-label">Name</label>
                            <input className="input w-input" name="name" data-name="Name" placeholder=""
                                type="text" id="name" />
                        </div>
                        <div className="form-group">
                            <label className="form-label">Password</label>
                            <input className="input w-input" name="name" data-name="Name" placeholder=""
                                type="text" id="name" />
                        </div>
                        <input type="submit" data-wait="Please wait..." className="primary-button w-button"
                            value="Submit" />
                        <div className="div-block">
                            <div className="secondary-color">Don &#x27;t have an account?</div>
                            <a className="link">Sign up</a>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </section>

  );
};

export default Login;