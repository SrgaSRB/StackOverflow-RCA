import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

const CreatePost = () => {
    const navigate = useNavigate();
    const [title, setTitle] = useState('');
    const [description, setDescription] = useState('');
    const [picture, setPicture] = useState<File | null>(null);
    const user = JSON.parse(localStorage.getItem('user') || '{}');

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!user.RowKey) {
            alert('You must be logged in to post a question.');
            navigate('/user-login');
            return;
        }

        const formData = new FormData();
        formData.append('Title', title);
        formData.append('Description', description);
        formData.append('UserId', user.RowKey);
        if (picture) {
            formData.append('Picture', picture);
        }

        try {
            const response = await fetch('http://localhost:5167/api/questions', {
                method: 'POST',
                body: formData,
            });

            if (response.ok) {
                alert('Question posted successfully!');
                navigate('/dashboard');
            } else {
                const errorData = await response.json();
                alert(`Failed to post question: ${JSON.stringify(errorData)}`);
            }
        } catch (error) {
            console.error('Error posting question:', error);
            alert('An error occurred while posting the question.');
        }
    };

    const handleCancel = () => {
        navigate('/dashboard');
    };

    return (
        <section className="ask-question-section">
            <div className="w-layout-blockcontainer container w-container">
                <div className="ask-question-wrapper">
                    <div className="form-block w-form">
                        <form id="email-form" className="form ask-question-form" onSubmit={handleSubmit}>
                            <div className="form-group">
                                <div className="text-block-7">Ask a Question</div>
                                <div className="text-block-9">Get help from the community by asking a clear, detailed question.
                                </div>
                            </div>
                            <div className="form-group">
                                <label className="form-label">Title</label>
                                <input 
                                    className="input w-input" 
                                    name="title" 
                                    placeholder="What's your programming question? Be specific." 
                                    type="text"
                                    id="title" 
                                    value={title}
                                    onChange={(e) => setTitle(e.target.value)}
                                    required
                                />
                                <div className="text-block-8">Be specific and imagine you're asking a question to another
                                    person</div>
                            </div>
                            <div className="form-group">
                                <label className="form-label">Question Details</label>
                                <textarea
                                    placeholder="Be specific and imagine you're asking a question to another person"
                                    id="description" 
                                    name="description"
                                    className="input textarea w-input"
                                    value={description}
                                    onChange={(e) => setDescription(e.target.value)}
                                    required
                                ></textarea>
                                <div className="text-block-8">
                                    Include:<br />
                                    • What you've tried<br />
                                    • What exactly you want to happen<br />
                                    • What actually happens instead<br />• Any error messages you see
                                </div>
                            </div>
                            <div className="form-group">
                                <label className="form-label">
                                    Error screenshot<span className="text-span">(Optional)</span>
                                </label>
                                <input 
                                    type="file" 
                                    className="input" 
                                    onChange={(e) => setPicture(e.target.files ? e.target.files[0] : null)}
                                />
                                <div className="text-block-8">Upload a screenshot of the error or problem if applicable</div>
                            </div>
                            <div className="form-group">
                                <div className="div-block-4">
                                    <div className="text-block-10">
                                        Writing a good question<br />
                                    </div>
                                    <div className="text-block-11">
                                        • Summarize the problem in the title<br />
                                        • Describe what you've tried and what you expected to happen<br />
                                        • Include minimal, complete, reproducible code<br />
                                        • Use proper formatting and grammar<br />• Add relevant tags
                                    </div>
                                </div>
                            </div>
                            <div className="aq-form-buttons">
                                <input type="submit" data-wait="Please wait..." className="primary-button w-button"
                                    value="Post Your Question" />
                                <input type="button" data-wait="Please wait..."
                                    className="primary-button secondary--button w-button" value="Cancel" onClick={handleCancel} />
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </section>
    );
};

export default CreatePost;