import { useEffect, useState } from "react";
import { useSearchParams, Link } from "react-router-dom";

interface UserInfo {
    Username: string;
    ProfilePictureUrl: string | null;
    QuestionsCount?: number;
}

interface QuestionDetails {
    QuestionId: string;
    Title: string;
    Description: string;
    Upvotes: number;
    Downvotes: number;
    TotalVotes: number;
    CreatedAt: string | Date;
    User: UserInfo;
    AnswersCount: number;
    PictureUrl?: string;
}

interface VoteState {
    upvotes: number;
    downvotes: number;
    totalVotes: number;
    userVote?: string | null;
}

const Dashboard = () => {
    const [questions, setQuestions] = useState<QuestionDetails[]>([]);
    const [popularQuestions, setPopularQuestions] = useState<QuestionDetails[]>([]);
    const [searchParams] = useSearchParams();
    const [questionVoteStates, setQuestionVoteStates] = useState<Record<string, VoteState>>({});

    useEffect(() => {
        const fetchQuestions = async () => {
            try {
                const response = await fetch("http://localhost:59535/api/questions");
                if (response.ok) {
                    const data = await response.json();
                    const searchTerm = searchParams.get('search');
                    
                    // Get current user from localStorage
                    const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
                    const currentUserId = currentUser.RowKey || currentUser.rowKey;
                    
                    let filteredData = data;
                    console.log("Current User ID:", currentUserId);
                    console.log("Fetched Questions:", data);
                    
                    // Filter out current user's questions
                    if (currentUserId) {
                        filteredData = data.filter((question: QuestionDetails) => {
                            // Assuming the API returns userId in the question object or user.id
                            // You might need to adjust this based on your actual API response structure
                            return question.User && question.User.Username !== currentUser.Username;
                        });
                    }
                    
                    // Apply search filter if provided
                    if (searchTerm) {
                        filteredData = filteredData.filter((question: QuestionDetails) =>
                            question.Title.toLowerCase().includes(searchTerm.toLowerCase())
                        );
                    }

                    const sortedData = filteredData.sort((a: QuestionDetails, b: QuestionDetails) => new Date(b.CreatedAt).getTime() - new Date(a.CreatedAt).getTime());
                    setQuestions(sortedData);

                    // Initialize vote states for all questions
                    const initialVoteStates: Record<string, VoteState> = {};
                    for (const question of sortedData) {
                        initialVoteStates[question.QuestionId] = {
                            upvotes: question.Upvotes || 0,
                            downvotes: question.Downvotes || 0,
                            totalVotes: question.TotalVotes || 0,
                            userVote: null
                        };
                    }
                    setQuestionVoteStates(initialVoteStates);

                    // Load user votes for all questions
                    if (currentUserId) {
                        for (const question of sortedData) {
                            fetchUserQuestionVote(currentUserId, question.QuestionId);
                        }
                    }
                } else {
                    console.error("Failed to fetch questions");
                }
            } catch (error) {
                console.error("Error fetching questions:", error);
            }
        };

        fetchQuestions();
    }, [searchParams]);

    const fetchPopularQuestions = async () => {
        try {
            const response = await fetch("http://localhost:59535/api/questions/popular?limit=100"); // Fetch many questions
            if (response.ok) {
                const data = await response.json();
                
                // Get current user from localStorage
                const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
                const currentUserId = currentUser.RowKey || currentUser.rowKey;
                
                let filteredData = data;
                
                // Filter out current user's questions
                if (currentUserId) {
                    filteredData = data.filter((question: QuestionDetails) => {
                        return question.User && question.User.Username !== currentUser.Username;
                    });
                }
                
                // Show all filtered questions (no slice limit)
                setPopularQuestions(filteredData);
            } else {
                console.error("Failed to fetch popular questions");
            }
        } catch (error) {
            console.error("Error fetching popular questions:", error);
        }
    };

    // Fetch popular questions on component mount
    useEffect(() => {
        fetchPopularQuestions();
    }, []);

    const fetchUserQuestionVote = async (userId: string, questionId: string) => {
        try {
            const response = await fetch(`http://localhost:59535/api/questions/${questionId}/vote/${userId}`);
            if (response.ok) {
                const data = await response.json();
                setQuestionVoteStates(prev => ({
                    ...prev,
                    [questionId]: {
                        ...prev[questionId],
                        userVote: data.userVote
                    }
                }));
            }
        } catch (error) {
            console.error("Error fetching user question vote:", error);
        }
    };

    const handleQuestionUpvote = async (questionId: string) => {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        const userId = user.RowKey || user.rowKey;

        if (!userId) {
            alert("You must be logged in to vote.");
            return;
        }

        try {
            const response = await fetch(`http://localhost:59535/api/questions/${questionId}/upvote`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ UserId: userId }),
            });

            if (response.ok) {
                const data = await response.json();
                setQuestionVoteStates(prev => ({
                    ...prev,
                    [questionId]: {
                        upvotes: data.upvotes,
                        downvotes: data.downvotes,
                        totalVotes: data.totalVotes,
                        userVote: data.userVote
                    }
                }));
                
                // Update the question in the list
                setQuestions(prev => prev.map(q => 
                    q.QuestionId === questionId 
                        ? { ...q, upvotes: data.upvotes, downvotes: data.downvotes, totalVotes: data.totalVotes }
                        : q
                ));

                // Update popular questions as well
                setPopularQuestions(prev => prev.map(q => 
                    q.QuestionId === questionId 
                        ? { ...q, upvotes: data.upvotes, downvotes: data.downvotes, totalVotes: data.totalVotes }
                        : q
                ));

                // Refresh popular questions to ensure proper ordering
                fetchPopularQuestions();
            } else {
                console.error("Failed to upvote question");
            }
        } catch (error) {
            console.error("Error upvoting question:", error);
        }
    };

    const handleQuestionDownvote = async (questionId: string) => {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        const userId = user.RowKey || user.rowKey;

        if (!userId) {
            alert("You must be logged in to vote.");
            return;
        }

        try {
            const response = await fetch(`http://localhost:59535/api/questions/${questionId}/downvote`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ UserId: userId }),
            });

            if (response.ok) {
                const data = await response.json();
                setQuestionVoteStates(prev => ({
                    ...prev,
                    [questionId]: {
                        upvotes: data.upvotes,
                        downvotes: data.downvotes,
                        totalVotes: data.totalVotes,
                        userVote: data.userVote
                    }
                }));
                
                // Update the question in the list
                setQuestions(prev => prev.map(q => 
                    q.QuestionId === questionId 
                        ? { ...q, Upvotes: data.upvotes, Downvotes: data.downvotes, TotalVotes: data.totalVotes }
                        : q
                ));

                // Update popular questions as well
                setPopularQuestions(prev => prev.map(q => 
                    q.QuestionId === questionId 
                        ? { ...q, Upvotes: data.upvotes, Downvotes: data.downvotes, TotalVotes: data.totalVotes }
                        : q
                ));

                // Refresh popular questions to ensure proper ordering
                fetchPopularQuestions();
            } else {
                console.error("Failed to downvote question");
            }
        } catch (error) {
            console.error("Error downvoting question:", error);
        }
    };

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
                            {questions.map((question) => {
                                const voteState = questionVoteStates[question.QuestionId] || {
                                    upvotes: question.Upvotes || 0,
                                    downvotes: question.Downvotes || 0,
                                    totalVotes: question.TotalVotes || 0,
                                    userVote: null
                                };
                                
                                return (
                                <div className="question-div" key={question.QuestionId}>
                                    <div className="question-left-side-info">
                                        <div className="question-left-side-info-div">
                                            <div className="question-left-side-info-top-div">
                                                <button 
                                                    onClick={() => handleQuestionUpvote(question.QuestionId)}
                                                    style={{
                                                        background: 'none',
                                                        border: 'none',
                                                        cursor: 'pointer',
                                                        opacity: voteState.userVote === 'upvote' ? 1 : 0.6
                                                    }}
                                                >
                                                    <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786ab9ccef31e64f760b7_upload.png"
                                                        loading="lazy" alt="Upvote" className="image" />
                                                </button>
                                                <div className="text-block-3">{voteState.upvotes - voteState.downvotes}</div>
                                                <button 
                                                    onClick={() => handleQuestionDownvote(question.QuestionId)}
                                                    style={{
                                                        background: 'none',
                                                        border: 'none',
                                                        cursor: 'pointer',
                                                        opacity: voteState.userVote === 'downvote' ? 1 : 0.6
                                                    }}
                                                >
                                                    <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786aaa1593ccde3ae5f09_download%20(1).png"
                                                        loading="lazy" alt="Downvote" className="image" />
                                                </button>
                                            </div>
                                            <div className="text-block-4">votes</div>
                                        </div>
                                        <div className="question-left-side-info-div">
                                            <div className="question-left-side-info-top-div">
                                                <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786aa1837efdbc1ee1afa_chat.png"
                                                    loading="lazy" alt="" className="image" />
                                                <div className="text-block-3">{question.AnswersCount}</div>
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
                                        <Link to={`/post/${question.QuestionId}`} className="question-title-link" style={{ textDecoration: 'none' }}>
                                            <div className="question-title">{question.Title}</div>
                                        </Link>
                                        <div className="question-description">{question.Description}</div>
                                        <div className="question-footer">
                                            <div className="question-user-div">
                                                <img src={question.User.ProfilePictureUrl || "https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78893518a2aa043ff4749_female-placeholder.webp"}
                                                    loading="lazy"
                                                    alt="" className="question-user-photo" />
                                                <div className="question-user-username">{question.User.Username}</div>
                                            </div>
                                            <div className="question-date-div">
                                                <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a789647d68e02a34817ecb_date.png"
                                                    loading="lazy" alt="" className="image-2" />
                                                <div>{new Date(question.CreatedAt).toLocaleDateString()}</div>
                                            </div>
                                        </div>
                                        {question.AnswersCount > 0 && (
                                            <div className="question-isanswered">
                                                <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a84da09c4a82a45ae94301_checked%20(2).png"
                                                    loading="lazy" alt="" className="image-6" />
                                                <div>Answered</div>
                                            </div>
                                        )}
                                    </div>
                                </div>
                                );
                            })}
                        </div>
                        <div className="popular-question-block">
                            <div className="popular-question-header">
                                <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78bab6971dd5e4ad490e4_trend%20(1).png"
                                    loading="lazy" alt="" className="image-3" />
                                <div className="text-block-5">Popular Questions</div>
                            </div>
                            <div className="popular-question-list">
                                {popularQuestions.map((question) => (
                                    <div className="popular-question-div" key={question.QuestionId}>
                                        <Link to={`/post/${question.QuestionId}`} style={{ textDecoration: 'none', color: 'inherit' }}>
                                            <div className="text-block-6">{question.Title}</div>
                                        </Link>
                                        <div className="popular-question-div-footer">
                                            <div className="popular-question-votes">{question.Upvotes - question.Downvotes}</div>
                                            <div>votes</div>
                                            <div className="div-block-2"></div>
                                            <div className="popular-question-answers">{question.AnswersCount}</div>
                                            <div>answers</div>
                                        </div>
                                        <div className="popular-question-hr"></div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    );

};

export default Dashboard;