import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';

interface UserInfo {
    username: string;
    profilePictureUrl: string | null;
    questionsCount: number;
}

interface Answer {
    answerId: string;
    content: string;
    createdAt: string;
    user: UserInfo;
    upvotes: number;
    downvotes: number;
    totalVotes: number;
    userQuestionsCount?: number;
}

interface QuestionDetails {
    questionId: string;
    title: string;
    description: string;
    pictureUrl?: string;
    upvotes: number;
    downvotes: number;
    totalVotes: number;
    createdAt: string | Date;
    user: UserInfo;
    answers: Answer[];
}

interface VoteState {
    upvotes: number;
    downvotes: number;
    totalVotes: number;
    userVote?: string | null;
}

const Post = () => {
    const { postId } = useParams<{ postId: string }>();
    const [question, setQuestion] = useState<QuestionDetails | null>(null);
    const [newAnswer, setNewAnswer] = useState('');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [showImageModal, setShowImageModal] = useState(false);
    const [authorQuestionsCount, setAuthorQuestionsCount] = useState<number>(0);
    const [answerAuthorsQuestionsCount, setAnswerAuthorsQuestionsCount] = useState<Record<string, number>>({});
    const [questionVoteState, setQuestionVoteState] = useState<VoteState>({
        upvotes: 0,
        downvotes: 0,
        totalVotes: 0,
        userVote: null
    });
    const [answerVoteStates, setAnswerVoteStates] = useState<Record<string, VoteState>>({});
    const [isQuestionAuthor, setIsQuestionAuthor] = useState<boolean>(false);

    useEffect(() => {
        // Get current user info once at the beginning
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        const userId = user.RowKey || user.rowKey;
        const currentUserUsername = user.Username || user.username;
        
        const fetchQuestion = async () => {
            try {
                console.log('Fetching question with ID:', postId);
                const response = await fetch(`http://localhost:5167/api/questions/${postId}`);
                console.log('Response status:', response.status);
                console.log('Response headers:', response.headers);
                
                if (response.ok) {
                    const data = await response.json();
                    console.log('Question data received:', data);
                    console.log('Question user:', data.user);
                    console.log('Question answers:', data.answers);
                    setQuestion(data);
                    
                    // Check if current user is the author of the question
                    if (userId && data.user && data.user.username) {
                        setIsQuestionAuthor(currentUserUsername === data.user.username);
                    }
                    
                    // Set initial question vote state
                    setQuestionVoteState({
                        upvotes: data.upvotes || 0,
                        downvotes: data.downvotes || 0,
                        totalVotes: data.totalVotes || 0,
                        userVote: null
                    });

                    // Load user's current vote for question
                    if (userId) {
                        await fetchUserQuestionVote(userId);
                    }
                    
                    if (data.user && data.user.username) {
                        await fetchAuthorQuestionsCount(data.user.username);
                    }

                    if (data.answers && data.answers.length > 0) {
                        const counts: Record<string, number> = {};
                        const voteStates: Record<string, VoteState> = {};
                        
                        for (const answer of data.answers) {
                            if (answer.user && answer.user.username) {
                                const count = await fetchAuthorQuestionsCount(answer.user.username, false);
                                counts[answer.user.username] = count;
                            }
                            
                            // Initialize answer vote state
                            voteStates[answer.answerId] = {
                                upvotes: answer.upvotes || 0,
                                downvotes: answer.downvotes || 0,
                                totalVotes: answer.totalVotes || 0,
                                userVote: null
                            };

                            // Load user's current vote for answer
                            if (userId) {
                                await fetchUserAnswerVote(userId, answer.answerId);
                            }
                        }
                        setAnswerAuthorsQuestionsCount(counts);
                        setAnswerVoteStates(voteStates);
                    }
                } else {
                    const errorText = await response.text();
                    console.error("Failed to fetch question:", response.status, errorText);
                    setError(`Failed to load question: ${response.status}`);
                }
            } catch (error) {
                console.error("Error fetching question:", error);
                setError("Network error occurred while loading question");
            } finally {
                setLoading(false);
            }
        };

        const fetchUserQuestionVote = async (userId: string) => {
            try {
                const response = await fetch(`http://localhost:5167/api/questions/${postId}/vote/${userId}`);
                if (response.ok) {
                    const data = await response.json();
                    setQuestionVoteState(prev => ({
                        ...prev,
                        userVote: data.userVote
                    }));
                }
            } catch (error) {
                console.error("Error fetching user question vote:", error);
            }
        };

        const fetchUserAnswerVote = async (userId: string, answerId: string) => {
            try {
                const response = await fetch(`http://localhost:5167/api/comments/${answerId}/vote/${userId}`);
                if (response.ok) {
                    const data = await response.json();
                    setAnswerVoteStates(prev => ({
                        ...prev,
                        [answerId]: {
                            ...prev[answerId],
                            userVote: data.userVote
                        }
                    }));
                }
            } catch (error) {
                console.error("Error fetching user answer vote:", error);
            }
        };

        const fetchAuthorQuestionsCount = async (username: string, isPostAuthor: boolean = true) => {
            try {
                const response = await fetch('http://localhost:5167/api/questions');
                if (response.ok) {
                    const allQuestions = await response.json();
                    const userQuestions = allQuestions.filter((q: any) => q.user && q.user.username === username);
                    if (isPostAuthor) {
                        setAuthorQuestionsCount(userQuestions.length);
                    }
                    return userQuestions.length;
                }
                return 0;
            } catch (error) {
                console.error("Error fetching author questions count:", error);
                if (isPostAuthor) {
                    setAuthorQuestionsCount(0);
                }
                return 0;
            }
        };

        if (postId) {
            fetchQuestion();
        }
    }, [postId]);

    const handleAnswerSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        const userId = user.RowKey || user.rowKey;

        if (!userId || !newAnswer.trim()) {
            alert("You must be logged in to post an answer.");
            return;
        }

        try {
            const response = await fetch(`http://localhost:5167/api/questions/${postId}/answers`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    Content: newAnswer,
                    UserId: userId,
                }),
            });

            if (response.ok) {
                const addedAnswer = await response.json();
                setQuestion(prev => prev ? { ...prev, answers: [...prev.answers, addedAnswer] } : null);
                setNewAnswer('');
                alert('Your answer has been posted successfully!');
            } else {
                console.error("Failed to post answer");
                alert('Failed to post answer. Please try again.');
            }
        } catch (error) {
            console.error("Error posting answer:", error);
            alert('An error occurred while posting your answer. Please try again.');
        }
    };

    const handleQuestionUpvote = async () => {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        const userId = user.RowKey || user.rowKey;

        if (!userId) {
            alert("You must be logged in to vote.");
            return;
        }

        try {
            const response = await fetch(`http://localhost:5167/api/questions/${postId}/upvote`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ UserId: userId }),
            });

            if (response.ok) {
                const data = await response.json();
                setQuestionVoteState({
                    upvotes: data.upvotes,
                    downvotes: data.downvotes,
                    totalVotes: data.totalVotes,
                    userVote: data.userVote
                });
                setQuestion(prev => prev ? { ...prev, totalVotes: data.totalVotes } : null);
            } else {
                console.error("Failed to upvote question");
            }
        } catch (error) {
            console.error("Error upvoting question:", error);
        }
    };

    const handleQuestionDownvote = async () => {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        const userId = user.RowKey || user.rowKey;

        if (!userId) {
            alert("You must be logged in to vote.");
            return;
        }

        try {
            const response = await fetch(`http://localhost:5167/api/questions/${postId}/downvote`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ UserId: userId }),
            });

            if (response.ok) {
                const data = await response.json();
                setQuestionVoteState({
                    upvotes: data.upvotes,
                    downvotes: data.downvotes,
                    totalVotes: data.totalVotes,
                    userVote: data.userVote
                });
                setQuestion(prev => prev ? { ...prev, totalVotes: data.totalVotes } : null);
            } else {
                console.error("Failed to downvote question");
            }
        } catch (error) {
            console.error("Error downvoting question:", error);
        }
    };

    const handleAnswerUpvote = async (answerId: string) => {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        const userId = user.RowKey || user.rowKey;

        if (!userId) {
            alert("You must be logged in to vote.");
            return;
        }

        try {
            const response = await fetch(`http://localhost:5167/api/comments/${answerId}/upvote`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ UserId: userId }),
            });

            if (response.ok) {
                const data = await response.json();
                setAnswerVoteStates(prev => ({
                    ...prev,
                    [answerId]: {
                        upvotes: data.upvotes,
                        downvotes: data.downvotes,
                        totalVotes: data.totalVotes,
                        userVote: data.userVote
                    }
                }));
                setQuestion(prev => {
                    if (!prev) return null;
                    const updatedAnswers = prev.answers.map(answer => 
                        answer.answerId === answerId 
                            ? { ...answer, totalVotes: data.totalVotes }
                            : answer
                    );
                    return { ...prev, answers: updatedAnswers };
                });
            } else {
                console.error("Failed to upvote answer");
            }
        } catch (error) {
            console.error("Error upvoting answer:", error);
        }
    };

    const handleAnswerDownvote = async (answerId: string) => {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        const userId = user.RowKey || user.rowKey;

        if (!userId) {
            alert("You must be logged in to vote.");
            return;
        }

        try {
            const response = await fetch(`http://localhost:5167/api/comments/${answerId}/downvote`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ UserId: userId }),
            });

            if (response.ok) {
                const data = await response.json();
                setAnswerVoteStates(prev => ({
                    ...prev,
                    [answerId]: {
                        upvotes: data.upvotes,
                        downvotes: data.downvotes,
                        totalVotes: data.totalVotes,
                        userVote: data.userVote
                    }
                }));
                setQuestion(prev => {
                    if (!prev) return null;
                    const updatedAnswers = prev.answers.map(answer => 
                        answer.answerId === answerId 
                            ? { ...answer, totalVotes: data.totalVotes }
                            : answer
                    );
                    return { ...prev, answers: updatedAnswers };
                });
            } else {
                console.error("Failed to downvote answer");
            }
        } catch (error) {
            console.error("Error downvoting answer:", error);
        }
    };

    if (loading) {
        return (
            <section className="question-section">
                <div className="w-layout-blockcontainer container w-container">
                    <div>Loading question...</div>
                </div>
            </section>
        );
    }

    if (error) {
        return (
            <section className="question-section">
                <div className="w-layout-blockcontainer container w-container">
                    <div>Error: {error}</div>
                </div>
            </section>
        );
    }

    if (!question || !question.user) {
        return (
            <section className="question-section">
                <div className="w-layout-blockcontainer container w-container">
                    <div>Question not found</div>
                </div>
            </section>
        );
    }

    return (
        <section className="question-section">
            <div className="w-layout-blockcontainer container w-container">
                <div className="question-wrapper">
                    <div className="q-question-block">
                        <div className="text-block-17">{question.title}</div>
                        <div className="div-block-6">
                            <div className="q-question-time">
                                <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a789647d68e02a34817ecb_date.png" loading="lazy" alt="" className="image-7" />
                                <div>Asked <span className="q-question-time-day">{new Date(question.createdAt).toLocaleDateString()}</span></div>
                            </div>
                        </div>
                        <div className="q-question-bottom-div">
                            <div className="q-question-bottom-left-div">
                                <div 
                                    className={`div-block-8 question-div-upvote ${questionVoteState.userVote === 'upvote' ? 'voted-up' : ''}`} 
                                    onClick={handleQuestionUpvote} 
                                    style={{ cursor: 'pointer' }}
                                >
                                    <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786ab9ccef31e64f760b7_upload.png" loading="lazy" alt="Upvote" className="image-8" />
                                </div>
                                <div className="text-block-18 upvote-count">{questionVoteState.upvotes}</div>
                                <div className="text-block-18 downvote-count">{questionVoteState.downvotes}</div>
                                <div 
                                    className={`div-block-8 question-div-downvote ${questionVoteState.userVote === 'downvote' ? 'voted-down' : ''}`} 
                                    onClick={handleQuestionDownvote} 
                                    style={{ cursor: 'pointer' }}
                                >
                                    <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786aaa1593ccde3ae5f09_download%20(1).png" loading="lazy" alt="Downvote" className="image-8" />
                                </div>
                            </div>
                            <div className="q-question-bottom-lright-div">
                                <div className="text-block-19">{question.description}</div>
                                {question.pictureUrl && (
                                    <div className="q-question-image-div">
                                        <img 
                                            src={question.pictureUrl} 
                                            alt="Question illustration" 
                                            className="image-9" 
                                        />
                                        <button 
                                            className="primary-button q-question-image-button w-button"
                                            onClick={() => setShowImageModal(true)}
                                        >
                                            View Full Image
                                        </button>
                                    </div>
                                )}
                            </div>
                        </div>
                        <div className="q-question-user-info">
                            <img 
                                src={question.user.profilePictureUrl || "https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78893518a2aa043ff4749_female-placeholder.webp"} 
                                loading="lazy" 
                                sizes="(max-width: 767px) 100vw, (max-width: 991px) 95vw, 940.0000610351562px"
                                srcSet={question.user.profilePictureUrl ? "" : "https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78893518a2aa043ff4749_female-placeholder-p-500.webp 500w, https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78893518a2aa043ff4749_female-placeholder-p-800.webp 800w, https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78893518a2aa043ff4749_female-placeholder.webp 1024w"}
                                alt="User profile picture" 
                                className="q-question-user-info-image" 
                            />
                            <div className="q-question-user-info-right-div">
                                <div className="q-question-user-info-username answer-user-username best-answer-user-username">@{question.user.username}</div>
                                <div className="text-block-20">({authorQuestionsCount})</div>
                            </div>
                        </div>
                    </div>
                    {!isQuestionAuthor && (
                        <div className="add-answer-block">
                            <div className="w-form">
                                <form id="email-form-4" className="form-3" onSubmit={handleAnswerSubmit}>
                                    <div className="text-block-23">Your Answer</div>
                                    <textarea
                                        placeholder="Write your answer here..."
                                        maxLength={5000}
                                        className="input answer-textarea w-input"
                                        value={newAnswer}
                                        onChange={(e) => setNewAnswer(e.target.value)}
                                    ></textarea>
                                    <input type="submit" value="Post Your Answer" className="primary-button add-answer-submit w-button" />
                                </form>
                            </div>
                        </div>
                    )}
                    <div className="q-answers-block">
                        <div className="text-block-21"><span className="q-answers-count">{question.answers?.length || 0}</span> Answers</div>
                        <div className="q-answers-list">
                            {question.answers && question.answers.length > 0 ? (
                                question.answers.map(answer => {
                                    const answerVoteState = answerVoteStates[answer.answerId] || { 
                                        upvotes: 0, 
                                        downvotes: 0, 
                                        totalVotes: answer.totalVotes, 
                                        userVote: null 
                                    };
                                    
                                    return (
                                    <div className="q-answer-div" key={answer.answerId}>
                                        <div className="q-answer-div-left-div">
                                            <div 
                                                className={`div-block-7 answer-div-upvote ${answerVoteState.userVote === 'upvote' ? 'voted-up' : ''}`} 
                                                onClick={() => handleAnswerUpvote(answer.answerId)} 
                                                style={{ cursor: 'pointer' }}
                                            >
                                                <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786ab9ccef31e64f760b7_upload.png" loading="lazy" alt="Upvote" className="image-10" />
                                            </div>
                                            <div className="q-answer-votes upvote-count">{answerVoteState.upvotes}</div>
                                            <div className="q-answer-votes downvote-count">{answerVoteState.downvotes}</div>
                                            <div 
                                                className={`div-block-7 answer-div-downvote ${answerVoteState.userVote === 'downvote' ? 'voted-down' : ''}`} 
                                                onClick={() => handleAnswerDownvote(answer.answerId)} 
                                                style={{ cursor: 'pointer' }}
                                            >
                                                <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786aaa1593ccde3ae5f09_download%20(1).png" loading="lazy" alt="Downvote" className="image-10" />
                                            </div>
                                        </div>
                                        <div className="q-answer-div-right-div">
                                            <div className="q-answer-content">{answer.content}</div>
                                            <div className="q-answer-right-bottom-div">
                                                <div className="q-answer-date-div">
                                                    <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a789647d68e02a34817ecb_date.png" loading="lazy" alt="" className="image-11" />
                                                    <div>
                                                        Answered <span className="q-answer-date">{new Date(answer.createdAt).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })} </span>
                                                        at <span className="q-answer-time">{new Date(answer.createdAt).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: true })}</span>
                                                    </div>
                                                </div>
                                                <div className="q-question-user-info answer-user-info">
                                                    <img src={answer.user.profilePictureUrl || "https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78893518a2aa043ff4749_female-placeholder.webp"} loading="lazy" alt="" className="q-question-user-info-image question-user-image" />
                                                    <div className="q-question-user-info-right-div">
                                                        <div className="q-question-user-info-username answer-user-username">@{answer.user.username}</div>
                                                        <div className="text-block-20">({answerAuthorsQuestionsCount[answer.user.username] || 0})</div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    );
                                })
                            ) : (
                                <div style={{ padding: '20px', textAlign: 'center', color: '#666' }}>
                                    No answers yet. Be the first to answer this question!
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            </div>
            
            {/* Image Modal */}
            {showImageModal && question?.pictureUrl && (
                <div style={{
                    position: 'fixed',
                    top: 0,
                    left: 0,
                    width: '100vw',
                    height: '100vh',
                    background: 'rgba(0,0,0,0.7)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    zIndex: 1000,
                }} onClick={() => setShowImageModal(false)}>
                    <img 
                        src={question.pictureUrl} 
                        alt="Question illustration - full size" 
                        style={{
                            maxWidth: '80vw',
                            maxHeight: '80vh',
                            borderRadius: '10px',
                        }}
                    />
                </div>
            )}
        </section>
    );
};

export default Post;