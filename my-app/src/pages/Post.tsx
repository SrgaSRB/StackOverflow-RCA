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
    totalVotes: number;
}

interface QuestionDetails {
    questionId: string;
    title: string;
    description: string;
    pictureUrl?: string;
    totalVotes: number;
    createdAt: string | Date;
    user: UserInfo;
    answers: Answer[];
}

const Post = () => {
    const { postId } = useParams<{ postId: string }>();
    const [question, setQuestion] = useState<QuestionDetails | null>(null);
    const [newAnswer, setNewAnswer] = useState('');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [showImageModal, setShowImageModal] = useState(false);
    const [authorQuestionsCount, setAuthorQuestionsCount] = useState<number>(0);

    useEffect(() => {
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
                    
                    // Fetch author's questions count
                    if (data.user && data.user.username) {
                        await fetchAuthorQuestionsCount(data.user.username);
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

        const fetchAuthorQuestionsCount = async (username: string) => {
            try {
                // Get all questions and count those by this user
                const response = await fetch('http://localhost:5167/api/questions');
                if (response.ok) {
                    const allQuestions = await response.json();
                    const userQuestions = allQuestions.filter((q: any) => q.user && q.user.username === username);
                    setAuthorQuestionsCount(userQuestions.length);
                }
            } catch (error) {
                console.error("Error fetching author questions count:", error);
                setAuthorQuestionsCount(0);
            }
        };

        if (postId) {
            fetchQuestion();
        }
    }, [postId]);

    const handleAnswerSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        const user = JSON.parse(localStorage.getItem('user') || '{}');

        if (!user.userId || !newAnswer.trim()) {
            // Handle not logged in or empty answer
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
                    UserId: user.userId,
                }),
            });

            if (response.ok) {
                const addedAnswer = await response.json();
                setQuestion(prev => prev ? { ...prev, answers: [...prev.answers, addedAnswer] } : null);
                setNewAnswer('');
            } else {
                console.error("Failed to post answer");
            }
        } catch (error) {
            console.error("Error posting answer:", error);
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
                                <div className="div-block-8"><img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786ab9ccef31e64f760b7_upload.png" loading="lazy" alt="" className="image-8" /></div>
                                <div className="text-block-18">{question.totalVotes}</div>
                                <div className="div-block-8"><img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786aaa1593ccde3ae5f09_download%20(1).png" loading="lazy" alt="" className="image-8" /></div>
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
                    <div className="add-answer-block">
                        <div className="w-form">
                            <form onSubmit={handleAnswerSubmit}>
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
                    <div className="q-answers-block">
                        <div className="text-block-21"><span className="q-answers-count">{question.answers?.length || 0}</span> Answers</div>
                        <div className="q-answers-list">
                            {question.answers && question.answers.length > 0 ? (
                                question.answers.map(answer => (
                                    <div className="q-answer-div" key={answer.answerId}>
                                        <div className="q-answer-div-left-div">
                                            <div className="div-block-7 answer-div-upvote"><img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786ab9ccef31e64f760b7_upload.png" loading="lazy" alt="" className="image-10" /></div>
                                            <div className="q-answer-votes">{answer.totalVotes}</div>
                                            <div className="div-block-7 answer-div-downvote"><img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a786aaa1593ccde3ae5f09_download%20(1).png" loading="lazy" alt="" className="image-10" /></div>
                                        </div>
                                        <div className="q-answer-div-right-div">
                                            <div className="q-answer-content">{answer.content}</div>
                                            <div className="q-answer-right-bottom-div">
                                                <div className="q-answer-date-div">
                                                    <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a789647d68e02a34817ecb_date.png" loading="lazy" alt="" className="image-11" />
                                                    <div>Answered <span className="q-answer-date">{new Date(answer.createdAt).toLocaleDateString()}</span></div>
                                                </div>
                                                <div className="q-question-user-info answer-user-info">
                                                    <img src={answer.user.profilePictureUrl || "https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78893518a2aa043ff4749_female-placeholder.webp"} loading="lazy" alt="" className="q-question-user-info-image question-user-image" />
                                                    <div className="q-question-user-info-right-div">
                                                        <div className="q-question-user-info-username answer-user-username">@{answer.user.username}</div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                ))
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