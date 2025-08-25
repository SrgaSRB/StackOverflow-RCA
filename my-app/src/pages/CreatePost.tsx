const CreatePost = () => {
    return (
        <section className="ask-question-section">
            <div className="w-layout-blockcontainer container w-container">
                <div className="ask-question-wrapper">
                    <div className="form-block w-form">
                        <form id="email-form" className="form ask-question-form">
                            <div className="form-group">
                                <div className="text-block-7">Ask a Question</div>
                                <div className="text-block-9">Get help from the community by asking a clear, detailed question.
                                </div>
                            </div>
                            <div className="form-group">
                                <label className="form-label">Title</label>
                                <input className="input w-input" name="name" data-name="Name"
                                    placeholder="What&#x27;s your programming question? Be specific." type="text"
                                    id="name" />
                                <div className="text-block-8">Be specific and imagine you &#x27;re asking a question to another
                                    person</div>
                            </div>
                            <div className="form-group">
                                <label className="form-label">Question Detalis</label>
                                <textarea
                                    placeholder="Be specific and imagine you&#x27;re asking a question to another person"
                                    id="field-4" name="field-4" data-name="Field 4"
                                    className="input textarea w-input"></textarea>
                                <div className="text-block-8">
                                    Include:<br />
                                    • What you &#x27;ve tried<br />
                                    • What exactly you want to happen<br />
                                    • What actually happens instead<br />• Any error messages you see
                                </div>
                            </div>
                            <div className="form-group">
                                <label className="form-label">
                                    Error screenshot<span className="text-span">(Optional)</span>
                                </label>
                                <div className="input">Upload Image input</div>
                                <div className="text-block-8">Upload a screenshot of the error or problem if applicable</div>
                            </div>
                            <div className="form-group">
                                <div className="div-block-4">
                                    <div className="text-block-10">
                                        Writing a good question<br />
                                    </div>
                                    <div className="text-block-11">
                                        • Summarize the problem in the title<br />
                                        • Describe what you &#x27;ve tried and what you expected to happen<br />
                                        • Include minimal, complete, reproducible code<br />
                                        • Use proper formatting and grammar<br />• Add relevant tags
                                    </div>
                                </div>
                            </div>
                            <div className="aq-form-buttons">
                                <input type="submit" data-wait="Please wait..." className="primary-button w-button"
                                    value="Post Your Question" />
                                <input type="submit" data-wait="Please wait..."
                                    className="primary-button secondary--button w-button" value="Cancel" />
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </section>
    );
};

export default CreatePost;