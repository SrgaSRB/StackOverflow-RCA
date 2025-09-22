import { useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import ReactCrop, {
  centerCrop,
  makeAspectCrop,
  type Crop,
  type PixelCrop,
} from "react-image-crop";
import "react-image-crop/dist/ReactCrop.css";

// Helper function to generate a cropped image file
function getCroppedImg(
  image: HTMLImageElement,
  crop: PixelCrop,
  fileName: string
): Promise<File> {
  const canvas = document.createElement("canvas");
  const scaleX = image.naturalWidth / image.width;
  const scaleY = image.naturalHeight / image.height;
  canvas.width = crop.width;
  canvas.height = crop.height;
  const ctx = canvas.getContext("2d");

  if (!ctx) {
    return Promise.reject(new Error("Failed to get 2D context"));
  }

  ctx.drawImage(
    image,
    crop.x * scaleX,
    crop.y * scaleY,
    crop.width * scaleX,
    crop.height * scaleY,
    0,
    0,
    crop.width,
    crop.height
  );

  return new Promise((resolve, reject) => {
    canvas.toBlob(
      (blob) => {
        if (!blob) {
          reject(new Error("Canvas is empty"));
          return;
        }
        const file = new File([blob], fileName, { type: blob.type });
        resolve(file);
      },
      "image/jpeg",
      0.95
    );
  });
}

interface Question {
  questionId: string;
  title: string;
  description: string;
  createdAt: string | Date;
  upvotes: number;
  downvotes: number;
  totalVotes: number;
  answersCount: number;
  pictureUrl?: string;
}

const Profile = () => {
  // Učitaj korisnika iz localStorage
  const [user, setUser] = useState(() => {
    const userData = JSON.parse(localStorage.getItem("user") || "{}");
    // Osiguraj da RowKey postoji
    if (!userData.RowKey && userData.rowKey) userData.RowKey = userData.rowKey;
    return userData;
  });
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState(user);
  const [showImageModal, setShowImageModal] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // State for cropping
  const [imgSrc, setImgSrc] = useState("");
  const imgRef = useRef<HTMLImageElement>(null);
  const [crop, setCrop] = useState<Crop>();
  const [completedCrop, setCompletedCrop] = useState<PixelCrop>();
  const [showCropModal, setShowCropModal] = useState(false);
  const [croppedImageFile, setCroppedImageFile] = useState<File | null>(null);
  const [croppedImagePreview, setCroppedImagePreview] = useState<string | null>(
    null
  );
  const [originalFileName, setOriginalFileName] = useState("");

  const [questions, setQuestions] = useState<Question[]>([]);
  const [userStats, setUserStats] = useState<{answersCount: number} | null>(null);
  const [showQuestionImageModal, setShowQuestionImageModal] = useState(false);
  const [selectedQuestionImageUrl, setSelectedQuestionImageUrl] = useState<
    string | null
  >(null);

  // Edit question states
  const [editingQuestionId, setEditingQuestionId] = useState<string | null>(
    null
  );
  const [editQuestionData, setEditQuestionData] = useState<{
    title: string;
    description: string;
  }>({ title: "", description: "" });
  const [editQuestionImage, setEditQuestionImage] = useState<File | null>(null);
  const [removeQuestionImage, setRemoveQuestionImage] = useState(false);
  const questionFileInputRef = useRef<HTMLInputElement>(null);

  const handleQuestionImageClick = (imageUrl: string) => {
    setSelectedQuestionImageUrl(imageUrl);
    setShowQuestionImageModal(true);
  };

  const handleQuestionImageModalClose = () => {
    setShowQuestionImageModal(false);
    setSelectedQuestionImageUrl(null);
  };

  // Question editing functions
  const handleEditQuestion = (question: Question) => {
    setEditingQuestionId(question.questionId);
    setEditQuestionData({
      title: question.title,
      description: question.description,
    });
    setEditQuestionImage(null);
    setRemoveQuestionImage(false);
  };

  const handleCancelEditQuestion = () => {
    setEditingQuestionId(null);
    setEditQuestionData({ title: "", description: "" });
    setEditQuestionImage(null);
    setRemoveQuestionImage(false);
  };

  const handleQuestionImageChange = (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    if (e.target.files && e.target.files.length > 0) {
      setEditQuestionImage(e.target.files[0]);
      setRemoveQuestionImage(false);
    }
  };

  const handleRemoveQuestionImage = () => {
    setRemoveQuestionImage(true);
    setEditQuestionImage(null);
    if (questionFileInputRef.current) {
      questionFileInputRef.current.value = "";
    }
  };

  const handleSaveQuestion = async (questionId: string) => {
    try {
      const formData = new FormData();
      formData.append("title", editQuestionData.title);
      formData.append("description", editQuestionData.description);
      formData.append("userId", user.RowKey);

      if (editQuestionImage) {
        formData.append("picture", editQuestionImage);
      }

      if (removeQuestionImage) {
        formData.append("removePicture", "true");
      }

      const response = await fetch(
        `http://localhost:59535/api/questions/${questionId}`,
        {
          method: "PUT",
          body: formData,
        }
      );

      if (response.ok) {
        await refreshQuestions(); // Refresh the entire list to get updated data
        handleCancelEditQuestion();
        alert("Question updated successfully!");
      } else {
        alert("Error updating question");
      }
    } catch (error) {
      console.error("Error updating question:", error);
      alert("Error updating question");
    }
  };

  const handleDeleteQuestion = async (questionId: string) => {
    if (window.confirm("Are you sure you want to delete this question?")) {
      try {
        const response = await fetch(
          `http://localhost:59535/api/questions/${questionId}?userId=${user.RowKey}`,
          {
            method: "DELETE",
          }
        );

        if (response.ok) {
          await refreshQuestions(); // Refresh the entire list
          alert("Question deleted successfully!");
        } else {
          alert("Error deleting question");
        }
      } catch (error) {
        console.error("Error deleting question:", error);
        alert("Error deleting question");
      }
    }
  };

  useEffect(() => {
    const fetchQuestions = async () => {
      if (user.RowKey) {
        try {
          const response = await fetch(
            `http://localhost:59535/api/questions/user/${user.RowKey}`
          );
          if (response.ok) {
            const data = await response.json();
            setQuestions(data);
          } else {
            console.error("Failed to fetch questions");
          }
        } catch (error) {
          console.error("Error fetching questions:", error);
        }
      }
    };

    const fetchUserStats = async () => {
      if (user.RowKey) {
        try {
          const response = await fetch(
            `http://localhost:59535/api/users/${user.RowKey}/stats`
          );
          if (response.ok) {
            const data = await response.json();
            setUserStats({ answersCount: data.answersCount });
          } else {
            console.error("Failed to fetch user stats");
          }
        } catch (error) {
          console.error("Error fetching user stats:", error);
        }
      }
    };

    fetchQuestions();
    fetchUserStats();
  }, [user.RowKey]);

  // Function to refresh questions
  const refreshQuestions = async () => {
    if (user.RowKey) {
      try {
        const response = await fetch(
          `http://localhost:59535/api/questions/user/${user.RowKey}`
        );
        if (response.ok) {
          const data = await response.json();
          setQuestions(data);
        }
      } catch (error) {
        console.error("Error refreshing questions:", error);
      }
    }
  };

  // Function to refresh user stats
  const refreshUserStats = async () => {
    if (user.RowKey) {
      try {
        const response = await fetch(
          `http://localhost:59535/api/users/${user.RowKey}/stats`
        );
        if (response.ok) {
          const data = await response.json();
          setUserStats({ answersCount: data.answersCount });
        }
      } catch (error) {
        console.error("Error refreshing user stats:", error);
      }
    }
  };

  // Kada se user promeni, osveži formData
  useEffect(() => {
    setFormData(user);
  }, [user]);

  // Edit mod
  const handleEdit = () => setIsEditing(true);

  // Promena polja
  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  // Čuvanje izmena
  const handleSave = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    const userId = formData.RowKey || formData.rowKey;
    if (!userId) {
      alert("Greška: Nema ID korisnika. Molimo ponovo se prijavite.");
      return;
    }

    let updatedUserData = { ...formData };

    try {
      // Ako postoji nova isečena slika za upload
      if (croppedImageFile) {
        const formDataImg = new FormData();
        formDataImg.append("file", croppedImageFile);

        const res = await fetch(
          `http://localhost:59535/api/users/${userId}/profile-picture`,
          {
            method: "POST",
            body: formDataImg,
          }
        );

        if (res.ok) {
          const data = await res.json();
          updatedUserData.profilePictureUrl = data.imageUrl;
        } else {
          alert("Greška pri upload-u slike");
          return;
        }
      }

      // Ažuriraj ostale podatke korisnika
      const response = await fetch(
        `http://localhost:59535/api/users/${userId}`,
        {
          method: "PUT",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(updatedUserData),
        }
      );

      if (response.ok) {
        const finalUpdatedUser = await response.json();
        localStorage.setItem("user", JSON.stringify(finalUpdatedUser));
        setUser(finalUpdatedUser);
        setFormData(finalUpdatedUser);
        setCroppedImageFile(null);
        setCroppedImagePreview(null);
        setImgSrc("");
        alert("Podaci uspešno ažurirani!");
      } else {
        alert("Greška pri ažuriranju korisnika");
      }
    } catch (error) {
      console.error("Greška pri čuvanju:", error);
      alert("Došlo je do greške pri čuvanju podataka.");
    }
    setIsEditing(false);
  };

  const handleCancel = () => {
    setFormData(user);
    setIsEditing(false);
    setCroppedImageFile(null);
    setCroppedImagePreview(null);
    setImgSrc("");
    setShowCropModal(false);
  };

  // Klik na sliku
  const handleImageClick = () => {
    if (isEditing) {
      fileInputRef.current?.click();
    } else {
      setShowImageModal(true);
    }
  };

  // Promena slike
  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setCrop(undefined); // Reset crop on new image
      const reader = new FileReader();
      const file = e.target.files[0];
      setOriginalFileName(file.name);
      reader.addEventListener("load", () =>
        setImgSrc(reader.result?.toString() || "")
      );
      reader.readAsDataURL(file);
      setShowCropModal(true);
    }
  };

  const handleModalClose = () => setShowImageModal(false);

  // Crop handlers
  function onImageLoad(e: React.SyntheticEvent<HTMLImageElement>) {
    const { width, height } = e.currentTarget;
    const crop = centerCrop(
      makeAspectCrop(
        {
          unit: "%",
          width: 90,
        },
        1, // 1:1 aspect ratio
        width,
        height
      ),
      width,
      height
    );
    setCrop(crop);
  }

  const handleCropConfirm = async () => {
    if (completedCrop?.width && completedCrop?.height && imgRef.current) {
      try {
        const croppedImgFile = await getCroppedImg(
          imgRef.current,
          completedCrop,
          originalFileName
        );
        setCroppedImageFile(croppedImgFile);
        setCroppedImagePreview(URL.createObjectURL(croppedImgFile));
        setShowCropModal(false);
      } catch (e) {
        console.error(e);
        alert("Greška pri isecanju slike.");
      }
    }
  };

  const handleCropCancel = () => {
    setShowCropModal(false);
    setImgSrc("");
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  return (
    <section className="user-profile-section">
      <div className="w-layout-blockcontainer container w-container">
        <div className="user-profile-wrapper">
          <div className="user-profile-left-div">
            <img
              src={
                croppedImagePreview ||
                formData.profilePictureUrl ||
                "https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78893518a2aa043ff4749_female-placeholder.webp"
              }
              loading="lazy"
              alt=""
              className="user-profile-user-image"
              onClick={handleImageClick}
              style={{ cursor: "pointer" }}
            />
            {isEditing && (
              <input
                type="file"
                ref={fileInputRef}
                style={{ display: "none" }}
                accept="image/*"
                onChange={handleImageChange}
              />
            )}
            {showImageModal && (
              <div
                style={{
                  position: "fixed",
                  top: 0,
                  left: 0,
                  width: "100vw",
                  height: "100vh",
                  background: "rgba(0,0,0,0.7)",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  zIndex: 1000,
                }}
                onClick={handleModalClose}
              >
                <img
                  src={
                    formData.profilePictureUrl ||
                    "https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78893518a2aa043ff4749_female-placeholder.webp"
                  }
                  alt="Profile enlarged"
                  style={{
                    maxWidth: "80vw",
                    maxHeight: "80vh",
                    borderRadius: "10px",
                  }}
                />
              </div>
            )}

            {/* Crop Modal */}
            {showCropModal && imgSrc && (
              <div
                style={{
                  position: "fixed",
                  top: 0,
                  left: 0,
                  width: "100%",
                  height: "100%",
                  background: "rgba(0,0,0,0.7)",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  zIndex: 1050,
                }}
              >
                <div
                  style={{
                    background: "white",
                    padding: "20px",
                    borderRadius: "10px",
                    textAlign: "center",
                  }}
                >
                  <h2>Crop Your Image</h2>
                  <ReactCrop
                    crop={crop}
                    onChange={(_: PixelCrop, percentCrop: Crop) =>
                      setCrop(percentCrop)
                    }
                    onComplete={(c: PixelCrop) => setCompletedCrop(c)}
                    aspect={1}
                  >
                    <img
                      ref={imgRef}
                      alt="Crop me"
                      src={imgSrc}
                      onLoad={onImageLoad}
                      style={{ maxHeight: "70vh" }}
                    />
                  </ReactCrop>
                  <div style={{ marginTop: "15px" }}>
                    <button
                      onClick={handleCropConfirm}
                      style={{ marginRight: "10px" }}
                    >
                      Confirm
                    </button>
                    <button onClick={handleCropCancel}>Cancel</button>
                  </div>
                </div>
              </div>
            )}

            <div>
              {isEditing ? (
                <form onSubmit={handleSave}>
                  <input
                    name="firstName"
                    value={formData.firstName || ""}
                    onChange={handleChange}
                    placeholder="First Name"
                  />
                  <input
                    name="lastName"
                    value={formData.lastName || ""}
                    onChange={handleChange}
                    placeholder="Last Name"
                  />
                  <input
                    name="username"
                    value={formData.username || ""}
                    onChange={handleChange}
                    placeholder="Username"
                  />
                  <input
                    name="email"
                    value={formData.email || ""}
                    onChange={handleChange}
                    placeholder="Email"
                  />
                  <input
                    name="country"
                    value={formData.country || ""}
                    onChange={handleChange}
                    placeholder="Country"
                  />
                  <input
                    name="city"
                    value={formData.city || ""}
                    onChange={handleChange}
                    placeholder="City"
                  />
                  <input
                    name="streetAddress"
                    value={formData.streetAddress || ""}
                    onChange={handleChange}
                    placeholder="Street Address"
                  />
                  <select
                    name="gender"
                    value={formData.gender || ""}
                    onChange={handleChange}
                  >
                    <option value="">Select Gender...</option>
                    <option value="male">Male</option>
                    <option value="female">Female</option>
                    <option value="other">Other</option>
                  </select>
                  <div
                    style={{ display: "flex", gap: "10px", marginTop: "10px" }}
                  >
                    <button type="submit">Save</button>
                    <button type="button" onClick={handleCancel}>
                      Cancel
                    </button>
                  </div>
                </form>
              ) : (
                <div>
                  <div className="user-profile-user-fullname">
                    {formData.firstName} {formData.lastName}
                  </div>
                  <div className="user-profile-user-username">
                    @{formData.username}
                  </div>
                  <button onClick={handleEdit}>Edit</button>
                </div>
              )}
            </div>
            <div className="user-profile-user-div">
              <img
                src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a79ad37e4df050bd8a8095_location.png"
                loading="lazy"
                alt=""
                className="image-5"
              />
              <div>
                {formData.country}, {formData.city}, {formData.streetAddress}
              </div>
            </div>
            <div className="user-profile-user-div">
              <img
                src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a789647d68e02a34817ecb_date.png"
                loading="lazy"
                alt=""
                className="image-5"
              />
              <div>
                Member since{" "}
                {formData.createdDate
                  ? new Date(formData.createdDate).toLocaleDateString()
                  : ""}
              </div>
            </div>
            <div className="user-profile-user-q-a-block">
              <div className="user-profile-user-q-a-div">
                <div className="user-profile-user-questions">
                  {questions.length}
                </div>
                <div className="text-block-12">Questions</div>
              </div>
              <div className="user-profile-user-q-a-div">
                <div className="user-profile-user-answers">
                  {userStats?.answersCount || 0}
                </div>
                <div className="text-block-12">Answers</div>
              </div>
            </div>
          </div>
          <div className="user-profile-main-div">
            <div className="user-profile-main-q-s-selector-div">
              <div className="user-profile-main-q-a">
                <div className="user-profile-main-q-a-tab-nav user-profile-main-q-a-tab-nav-select">
                  <div>Questions ({questions.length})</div>
                </div>
                <div className="user-profile-main-q-a-tab-nav">
                  <div>Answers ({userStats?.answersCount || 0})</div>
                </div>
              </div>
            </div>
            <div className="user-profile-q-a-list-div">
              {questions.map((question) => (
                <div key={question.questionId} className="user-profile-q-a-item">
                  <div
                    className="user-profile-q-a-div"
                    style={{
                      display: "flex",
                      alignItems: "flex-start",
                      width: "100%",
                    }}
                  >
                    {/* Stats and Actions Div */}
                    <div
                      className="user-profile-q-a-div-left"
                      style={{
                        display: "flex",
                        flexDirection: "column",
                        alignItems: "center",
                        gap: "10px",
                        width: "120px",
                        flexShrink: 0,
                      }}
                    >
                      <div
                        style={{
                          display: "flex",
                          gap: "15px",
                          justifyContent: "center",
                          width: "100%",
                        }}
                      >
                        <div
                          className="user-profile-q-a-div-left-info-div"
                          style={{ textAlign: "center" }}
                        >
                          <div className="text-block-15">
                            {question.totalVotes}
                          </div>
                          <div
                            className="text-block-16"
                            style={{ fontSize: "11px" }}
                          >
                            votes
                          </div>
                        </div>
                        <div
                          className="user-profile-q-a-div-left-info-div"
                          style={{ textAlign: "center" }}
                        >
                          <div className="text-block-15">{question.answersCount || 0}</div>
                          <div
                            className="text-block-16"
                            style={{ fontSize: "11px" }}
                          >
                            answers
                          </div>
                        </div>
                        <div
                          className="user-profile-q-a-div-left-info-div"
                          style={{ textAlign: "center" }}
                        >
                          <div className="text-block-15">0</div>
                          <div
                            className="text-block-16"
                            style={{ fontSize: "11px" }}
                          >
                            views
                          </div>
                        </div>
                      </div>
                      {/* Edit and Delete buttons */}
                      {editingQuestionId !== question.questionId && (
                        <div
                          style={{
                            display: "flex",
                            flexDirection: "row",
                            gap: "6px",
                          }}
                        >
                          <button
                            onClick={() => handleEditQuestion(question)}
                            style={{
                              backgroundColor: "white",
                              color: "#333",
                              border: "1px solid #ddd",
                              padding: "4px 8px",
                              borderRadius: "3px",
                              cursor: "pointer",
                              fontSize: "11px",
                              fontWeight: "500",
                              transition: "all 0.2s ease",
                              boxShadow: "0 1px 2px rgba(0,0,0,0.1)",
                              minWidth: "35px",
                              textAlign: "center",
                            }}
                            onMouseOver={(e) => {
                              e.currentTarget.style.backgroundColor = "#f8f9fa";
                              e.currentTarget.style.borderColor = "#adb5bd";
                            }}
                            onMouseOut={(e) => {
                              e.currentTarget.style.backgroundColor = "white";
                              e.currentTarget.style.borderColor = "#ddd";
                            }}
                          >
                            Edit
                          </button>
                          <button
                            onClick={() =>
                              handleDeleteQuestion(question.questionId)
                            }
                            style={{
                              backgroundColor: "#38ad73",
                              color: "white",
                              border: "none",
                              padding: "4px 8px",
                              borderRadius: "3px",
                              cursor: "pointer",
                              fontSize: "11px",
                              fontWeight: "500",
                              transition: "all 0.2s ease",
                              boxShadow: "0 1px 2px rgba(0,0,0,0.1)",
                              minWidth: "40px",
                              textAlign: "center",
                            }}
                            onMouseOver={(e) => {
                              e.currentTarget.style.backgroundColor =
                                "#145c38ff";
                            }}
                            onMouseOut={(e) => {
                              e.currentTarget.style.backgroundColor = "#38ad73";
                            }}
                          >
                            Delete
                          </button>
                        </div>
                      )}
                    </div>

                    {/* Info Div */}
                    <div
                      className="user-profile-q-a-div-info"
                      style={{ flex: 1, marginRight: "20px" }}
                    >
                      {editingQuestionId === question.questionId ? (
                        <div
                          style={{
                            padding: "15px",
                            border: "1px solid #e9ecef",
                            borderRadius: "8px",
                            backgroundColor: "#f8f9fa",
                          }}
                        >
                          <input
                            type="text"
                            value={editQuestionData.title}
                            onChange={(e) =>
                              setEditQuestionData({
                                ...editQuestionData,
                                title: e.target.value,
                              })
                            }
                            placeholder="Question title"
                            style={{
                              width: "100%",
                              marginBottom: "12px",
                              padding: "8px 12px",
                              border: "1px solid #ced4da",
                              borderRadius: "4px",
                              fontSize: "14px",
                            }}
                          />
                          <textarea
                            value={editQuestionData.description}
                            onChange={(e) =>
                              setEditQuestionData({
                                ...editQuestionData,
                                description: e.target.value,
                              })
                            }
                            placeholder="Question description"
                            rows={4}
                            style={{
                              width: "100%",
                              marginBottom: "12px",
                              padding: "8px 12px",
                              border: "1px solid #ced4da",
                              borderRadius: "4px",
                              fontSize: "14px",
                              resize: "vertical",
                            }}
                          />

                          <div style={{ marginBottom: "12px" }}>
                            <label
                              style={{
                                fontSize: "13px",
                                fontWeight: "500",
                                color: "#495057",
                                marginBottom: "6px",
                                display: "block",
                              }}
                            >
                              Update Image:
                            </label>
                            <input
                              type="file"
                              ref={questionFileInputRef}
                              accept="image/*"
                              onChange={handleQuestionImageChange}
                              style={{
                                display: "block",
                                marginTop: "5px",
                                fontSize: "13px",
                              }}
                            />
                            {question.pictureUrl && !removeQuestionImage && (
                              <div style={{ marginTop: "8px" }}>
                                <button
                                  type="button"
                                  onClick={handleRemoveQuestionImage}
                                  style={{
                                    backgroundColor: "#145c38ff",
                                    color: "white",
                                    border: "none",
                                    padding: "4px 8px",
                                    borderRadius: "3px",
                                    cursor: "pointer",
                                    fontSize: "11px",
                                  }}
                                >
                                  Remove Current Image
                                </button>
                              </div>
                            )}
                            {removeQuestionImage && (
                              <div
                                style={{
                                  color: "#dc3545",
                                  marginTop: "5px",
                                  fontSize: "12px",
                                }}
                              >
                                Current image will be removed
                              </div>
                            )}
                          </div>

                          <div style={{ display: "flex", gap: "10px" }}>
                            <button
                              onClick={() =>
                                handleSaveQuestion(question.questionId)
                              }
                              style={{
                                backgroundColor: "#38ad73",
                                color: "white",
                                border: "none",
                                padding: "8px 16px",
                                borderRadius: "4px",
                                cursor: "pointer",
                                fontSize: "13px",
                                fontWeight: "500",
                              }}
                            >
                              Save Changes
                            </button>
                            <button
                              onClick={handleCancelEditQuestion}
                              style={{
                                backgroundColor: "white",
                                color: "#6c757d",
                                border: "1px solid #ced4da",
                                padding: "8px 16px",
                                borderRadius: "4px",
                                cursor: "pointer",
                                fontSize: "13px",
                                fontWeight: "500",
                              }}
                            >
                              Cancel
                            </button>
                          </div>
                        </div>
                      ) : (
                        <>
                          <Link 
                            to={`/post/${question.questionId}`} 
                            style={{ textDecoration: 'none', color: 'inherit' }}
                          >
                            <div className="user-profile-q-a-div-info-title" style={{ cursor: 'pointer' }}>
                              {question.title}
                            </div>
                          </Link>
                          <div className="user-profile-q-a-div-info-description">
                            {question.description}
                          </div>
                          <div className="user-profile-q-a-div-info-date">
                            Asked{" "}
                            {new Date(question.createdAt).toLocaleDateString()}
                          </div>
                        </>
                      )}
                    </div>

                    {/* Image Div */}
                    {question.pictureUrl &&
                      editingQuestionId !== question.questionId &&
                      !removeQuestionImage && (
                        <div style={{ flexShrink: 0 }}>
                          <img
                            src={question.pictureUrl}
                            alt="Question"
                            style={{
                              width: "100px",
                              height: "100px",
                              objectFit: "cover",
                              cursor: "pointer",
                              borderRadius: "5px",
                            }}
                            onClick={() =>
                              question.pictureUrl &&
                              handleQuestionImageClick(question.pictureUrl)
                            }
                          />
                        </div>
                      )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
      {showQuestionImageModal && selectedQuestionImageUrl && (
        <div
          style={{
            position: "fixed",
            top: 0,
            left: 0,
            width: "100vw",
            height: "100vh",
            background: "rgba(0,0,0,0.7)",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            zIndex: 1001,
          }}
          onClick={handleQuestionImageModalClose}
        >
          <img
            src={selectedQuestionImageUrl}
            alt="Question enlarged"
            style={{
              maxWidth: "80vw",
              maxHeight: "80vh",
              borderRadius: "10px",
            }}
          />
        </div>
      )}
    </section>
  );
};

export default Profile;
