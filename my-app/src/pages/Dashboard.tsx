
const Dashboard = () => {

    return (
        <section className="dashboard-section">
            <div className="w-layout-blockcontainer container w-container">
                <div className="dashboard-wrapper">
                    <div className="dashboard-filter-block">
                        <div className="text-block-2">All Questions</div>
                        <div className="dashboard-filter-div">
                            <div className="form-block w-form">
                                <form id="email-form-3">
                                    <select id="field-3" name="field-3" data-name="Field 3" className="input w-select">
                                        <option value="">Select one...</option>
                                        <option value="First">First choice</option>
                                        <option value="Second">Second choice</option>
                                        <option value="Third">Third choice</option>
                                    </select>
                                </form>
                            </div>
                        </div>
                    </div>
                    <div className="dashboard-main-div">
                        <div className="dashboard-questions">
                            <div className="question-div">
                                <div className="question-left-side-info">
                                    <div className="question-left-side-info-div">
                                        <div className="question-left-side-info-top-div">
                                            <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786ab9ccef31e64f760b7_upload.png"
                                                loading="lazy" alt="" className="image" />
                                            <div className="text-block-3">12</div>
                                            <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786aaa1593ccde3ae5f09_download%20(1).png"
                                                loading="lazy" alt="" className="image" />
                                        </div>
                                        <div className="text-block-4">votes</div>
                                    </div>
                                    <div className="question-left-side-info-div">
                                        <div className="question-left-side-info-top-div">
                                            <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786aa1837efdbc1ee1afa_chat.png"
                                                loading="lazy" alt="" className="image" />
                                            <div className="text-block-3">12</div>
                                        </div>
                                        <div className="text-block-4">votes</div>
                                    </div>
                                    <div className="question-left-side-info-div">
                                        <div className="question-left-side-info-top-div">
                                            <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786aa3de95934e6da60e3_view.png"
                                                loading="lazy" alt="" className="image" />
                                            <div className="text-block-3">12</div>
                                        </div>
                                        <div className="text-block-4">votes</div>
                                    </div>
                                </div>
                                <div className="question-main-div">
                                    <div className="question-title">How to implement authentication in React with TypeScript?
                                    </div>
                                    <div className="question-description">Contrary to popular belief, Lorem Ipsum is not simply
                                        random text. It has roots in a piece of classical Latin literature from 45 BC,
                                        making it over 2000 years old. Richard McClintock, a Latin professor at
                                        Hampden-Sydney College in Virginia, looked up one of the more obscure Latin words,
                                        consectetur, from a Lorem Ipsum passage, and going through the cites of the word in
                                        classical literature, discovered the undoubtable source.</div>
                                    <div className="question-footer">
                                        <div className="question-user-div">
                                            <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78893518a2aa043ff4749_female-placeholder.webp"
                                                loading="lazy"
                                                sizes="(max-width: 767px) 100vw, (max-width: 991px) 95vw, 940.0000610351562px"
                                                alt="" className="question-user-photo" />
                                            <div className="question-user-username">srdjandelic02</div>
                                        </div>
                                        <div className="question-date-div">
                                            <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a789647d68e02a34817ecb_date.png"
                                                loading="lazy" alt="" className="image-2" />
                                            <div>21/08/2025</div>
                                        </div>
                                    </div>
                                    <div className="question-isanswered">
                                        <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a84da09c4a82a45ae94301_checked%20(2).png"
                                            loading="lazy" alt="" className="image-6" />
                                        <div>Answered</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div className="popular-question-block">
                            <div className="popular-question-header">
                                <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78bab6971dd5e4ad490e4_trend%20(1).png"
                                    loading="lazy" alt="" className="image-3" />
                                <div className="text-block-5">Popular Questions</div>
                            </div>
                            <div className="popular-question-list">
                                <div className="popular-question-div">
                                    <div className="text-block-6">How to optimize React app performance with large datasets?
                                    </div>
                                    <div className="popular-question-div-footer">
                                        <div className="popular-question-votes">22</div>
                                        <div>votes</div>
                                        <div className="div-block-2"></div>
                                        <div className="popular-question-answers">8</div>
                                        <div>answers</div>
                                    </div>
                                    <div className="popular-question-hr"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    );

};

export default Dashboard;