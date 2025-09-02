import { useEffect, useState } from "react";
import { useSearchParams, Link } from "react-router-dom";

interface UserInfo {
    username: string;
    profilePictureUrl: string | null;
}

interface QuestionDetails {
    questionId: string;
    title: string;
    description: string;
    totalVotes: number;
    createdAt: string | Date;
    user: UserInfo;
    answersCount: number;
}

const Dashboard = () => {
    const [questions, setQuestions] = useState<QuestionDetails[]>([]);
    const [searchParams] = useSearchParams();

    useEffect(() => {
        const fetchQuestions = async () => {
            try {
                const response = await fetch("http://localhost:5167/api/questions");
                if (response.ok) {
                    const data = await response.json();
                    const searchTerm = searchParams.get('search');
                    
                    // Get current user from localStorage
                    const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
                    const currentUserId = currentUser.RowKey || currentUser.rowKey;
                    
                    let filteredData = data;
                    
                    // Filter out current user's questions
                    if (currentUserId) {
                        filteredData = data.filter((question: QuestionDetails) => {
                            // Assuming the API returns userId in the question object or user.id
                            // You might need to adjust this based on your actual API response structure
                            return question.user && question.user.username !== currentUser.username;
                        });
                    }
                    
                    // Apply search filter if provided
                    if (searchTerm) {
                        filteredData = filteredData.filter((question: QuestionDetails) =>
                            question.title.toLowerCase().includes(searchTerm.toLowerCase())
                        );
                    }

                    const sortedData = filteredData.sort((a: QuestionDetails, b: QuestionDetails) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
                    setQuestions(sortedData);
                } else {
                    console.error("Failed to fetch questions");
                }
            } catch (error) {
                console.error("Error fetching questions:", error);
            }
        };

        fetchQuestions();
    }, [searchParams]);

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
                            {questions.map((question) => (
                                <div className="question-div" key={question.questionId}>
                                    <div className="question-left-side-info">
                                        <div className="question-left-side-info-div">
                                            <div className="question-left-side-info-top-div">
                                                <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786ab9ccef31e64f760b7_upload.png"
                                                    loading="lazy" alt="" className="image" />
                                                <div className="text-block-3">{question.totalVotes}</div>
                                                <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786aaa1593ccde3ae5f09_download%20(1).png"
                                                    loading="lazy" alt="" className="image" />
                                            </div>
                                            <div className="text-block-4">votes</div>
                                        </div>
                                        <div className="question-left-side-info-div">
                                            <div className="question-left-side-info-top-div">
                                                <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786aa1837efdbc1ee1afa_chat.png"
                                                    loading="lazy" alt="" className="image" />
                                                <div className="text-block-3">{question.answersCount}</div>
                                            </div>
                                            <div className="text-block-4">answers</div>
                                        </div>
                                        <div className="question-left-side-info-div">
                                            <div className="question-left-side-info-top-div">
                                                <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786aa3de95934e6da60e3_view.png"
                                                    loading="lazy" alt="" className="image" />
                                                <div className="text-block-3">0</div>
                                            </div>
                                            <div className="text-block-4">views</div>
                                        </div>
                                    </div>
                                    <div className="question-main-div">
                                        <Link to={`/post/${question.questionId}`} className="question-title-link">
                                            <div className="question-title">{question.title}</div>
                                        </Link>
                                        <div className="question-description">{question.description}</div>
                                        <div className="question-footer">
                                            <div className="question-user-div">
                                                <img src={question.user.profilePictureUrl || "https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78893518a2aa043ff4749_female-placeholder.webp"}
                                                    loading="lazy"
                                                    alt="" className="question-user-photo" />
                                                <div className="question-user-username">{question.user.username}</div>
                                            </div>
                                            <div className="question-date-div">
                                                <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a789647d68e02a34817ecb_date.png"
                                                    loading="lazy" alt="" className="image-2" />
                                                <div>{new Date(question.createdAt).toLocaleDateString()}</div>
                                            </div>
                                        </div>
                                        {question.answersCount > 0 && (
                                            <div className="question-isanswered">
                                                <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a84da09c4a82a45ae94301_checked%20(2).png"
                                                    loading="lazy" alt="" className="image-6" />
                                                <div>Answered</div>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            ))}
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