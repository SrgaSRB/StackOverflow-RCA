import { createBrowserRouter, RouterProvider, Navigate } from 'react-router-dom';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import Post from './pages/Post';
import Profile from './pages/Profile';
import CreatePost from './pages/CreatePost';
import Login from './pages/Login';
import PublicLayout from './layout/PublicLayout';
import NotFound from './components/shared/NotFound';
import PrivateLayout from './layout/PrivateLayout';

const router = createBrowserRouter([
  { path: "/", element: <Navigate to="/dashboard" replace /> },

  {
    element: <PublicLayout />,
    children: [
      { path: "/user-login", element: <Login /> },
      { path: "/user-register", element: <Register /> },
    ],
  },

  {
    element: <PrivateLayout />,
    children: [
      { path: "/dashboard", element: <Dashboard /> },
      { path: "/create-post", element: <CreatePost /> },
      { path: "/update-post/:id", element: <CreatePost /> },
      { path: "/post/:postId", element: <Post /> },
      { path: "/profile/:id", element: <Profile /> },
      { path: "/profile", element: <Profile /> }, //my-profile
    ],
  },

  { path: "*", element: <NotFound /> },
]);

const App = () => <RouterProvider router={router} />;

export default App;