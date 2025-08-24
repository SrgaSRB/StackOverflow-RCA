import { Link } from 'react-router-dom';

const Post = () => {
  return (
    <div className="post-container">
      <h1>Post Title</h1>
      <div>
        {/* Post content */}
      </div>
      <div>
        <textarea placeholder="Write your answer here..." />
        <button>Post Your Answer</button>
      </div>
      <div>
        {/* Answers */}
      </div>
    </div>
  );
};

export default Post;