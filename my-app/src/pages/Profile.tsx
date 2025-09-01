import { useState, useRef, useEffect } from 'react';
import ReactCrop, {
    centerCrop,
    makeAspectCrop,
    type Crop,
    type PixelCrop,
} from 'react-image-crop';
import 'react-image-crop/dist/ReactCrop.css';

// Helper function to generate a cropped image file
function getCroppedImg(
    image: HTMLImageElement,
    crop: PixelCrop,
    fileName: string
): Promise<File> {
    const canvas = document.createElement('canvas');
    const scaleX = image.naturalWidth / image.width;
    const scaleY = image.naturalHeight / image.height;
    canvas.width = crop.width;
    canvas.height = crop.height;
    const ctx = canvas.getContext('2d');

    if (!ctx) {
        return Promise.reject(new Error('Failed to get 2D context'));
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
                    reject(new Error('Canvas is empty'));
                    return;
                }
                const file = new File([blob], fileName, { type: blob.type });
                resolve(file);
            },
            'image/jpeg',
            0.95
        );
    });
}

interface Question {
    rowKey: string;
    title: string;
    description: string;
    timestamp: string;
    upvote: number;
    downvote: number;
    totalVotes: number;
    pictureUrl?: string;
}

const Profile = () => {
    // Učitaj korisnika iz localStorage
    const [user, setUser] = useState(() => {
        const userData = JSON.parse(localStorage.getItem('user') || '{}');
        // Osiguraj da RowKey postoji
        if (!userData.RowKey && userData.rowKey) userData.RowKey = userData.rowKey;
        return userData;
    });
    const [isEditing, setIsEditing] = useState(false);
    const [formData, setFormData] = useState(user);
    const [showImageModal, setShowImageModal] = useState(false);
    const fileInputRef = useRef<HTMLInputElement>(null);
    
    // State for cropping
    const [imgSrc, setImgSrc] = useState('');
    const imgRef = useRef<HTMLImageElement>(null);
    const [crop, setCrop] = useState<Crop>();
    const [completedCrop, setCompletedCrop] = useState<PixelCrop>();
    const [showCropModal, setShowCropModal] = useState(false);
    const [croppedImageFile, setCroppedImageFile] = useState<File | null>(null);
    const [croppedImagePreview, setCroppedImagePreview] = useState<string | null>(null);
    const [originalFileName, setOriginalFileName] = useState('');

    const [questions, setQuestions] = useState<Question[]>([]);
    const [showQuestionImageModal, setShowQuestionImageModal] = useState(false);
    const [selectedQuestionImageUrl, setSelectedQuestionImageUrl] = useState<string | null>(null);

    const handleQuestionImageClick = (imageUrl: string) => {
        setSelectedQuestionImageUrl(imageUrl);
        setShowQuestionImageModal(true);
    };

    const handleQuestionImageModalClose = () => {
        setShowQuestionImageModal(false);
        setSelectedQuestionImageUrl(null);
    };

    useEffect(() => {
        const fetchQuestions = async () => {
            if (user.RowKey) {
                try {
                    const response = await fetch(`http://localhost:5167/api/questions/user/${user.RowKey}`);
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

        fetchQuestions();
    }, [user.RowKey]);

    // Kada se user promeni, osveži formData
    useEffect(() => {
        setFormData(user);
    }, [user]);

    // Edit mod
    const handleEdit = () => setIsEditing(true);

    // Promena polja
    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    // Čuvanje izmena
    const handleSave = async (e?: React.FormEvent) => {
        if (e) e.preventDefault();
        const userId = formData.RowKey || formData.rowKey;
        if (!userId) {
            alert('Greška: Nema ID korisnika. Molimo ponovo se prijavite.');
            return;
        }

        let updatedUserData = { ...formData };

        try {
            // Ako postoji nova isečena slika za upload
            if (croppedImageFile) {
                const formDataImg = new FormData();
                formDataImg.append('file', croppedImageFile);

                const res = await fetch(`http://localhost:5167/api/users/${userId}/profile-picture`, {
                    method: 'POST',
                    body: formDataImg
                });

                if (res.ok) {
                    const data = await res.json();
                    updatedUserData.profilePictureUrl = data.imageUrl;
                } else {
                    alert('Greška pri upload-u slike');
                    return; 
                }
            }

            // Ažuriraj ostale podatke korisnika
            const response = await fetch(`http://localhost:5167/api/users/${userId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(updatedUserData)
            });

            if (response.ok) {
                const finalUpdatedUser = await response.json();
                localStorage.setItem('user', JSON.stringify(finalUpdatedUser));
                setUser(finalUpdatedUser);
                setFormData(finalUpdatedUser);
                setCroppedImageFile(null);
                setCroppedImagePreview(null);
                setImgSrc('');
                alert('Podaci uspešno ažurirani!');
            } else {
                alert('Greška pri ažuriranju korisnika');
            }
        } catch (error) {
            console.error("Greška pri čuvanju:", error);
            alert('Došlo je do greške pri čuvanju podataka.');
        }
        setIsEditing(false);
    };

    const handleCancel = () => {
        setFormData(user);
        setIsEditing(false);
        setCroppedImageFile(null);
        setCroppedImagePreview(null);
        setImgSrc('');
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
            reader.addEventListener('load', () =>
                setImgSrc(reader.result?.toString() || '')
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
                    unit: '%',
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
                alert('Greška pri isecanju slike.');
            }
        }
    };

    const handleCropCancel = () => {
        setShowCropModal(false);
        setImgSrc('');
        if (fileInputRef.current) {
            fileInputRef.current.value = '';
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
                            style={{ cursor: 'pointer' }}
                        />
                        {isEditing && (
                            <input
                                type="file"
                                ref={fileInputRef}
                                style={{ display: 'none' }}
                                accept="image/*"
                                onChange={handleImageChange}
                            />
                        )}
                        {showImageModal && (
                            <div
                                style={{
                                    position: 'fixed',
                                    top: 0,
                                    left: 0,
                                    width: '100vw',
                                    height: '100vh',
                                    background: 'rgba(0,0,0,0.7)',
                                    display: 'flex',
                                    alignItems: 'center',
                                    justifyContent: 'center',
                                    zIndex: 1000
                                }}
                                onClick={handleModalClose}
                            >
                                <img
                                    src={
                                        formData.profilePictureUrl ||
                                        "https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78893518a2aa043ff4749_female-placeholder.webp"
                                    }
                                    alt="Profile enlarged"
                                    style={{ maxWidth: '80vw', maxHeight: '80vh', borderRadius: '10px' }}
                                />
                            </div>
                        )}

                        {/* Crop Modal */}
                        {showCropModal && imgSrc && (
                            <div style={{
                                position: 'fixed', top: 0, left: 0, width: '100%', height: '100%',
                                background: 'rgba(0,0,0,0.7)', display: 'flex', alignItems: 'center',
                                justifyContent: 'center', zIndex: 1050
                            }}>
                                <div style={{ background: 'white', padding: '20px', borderRadius: '10px', textAlign: 'center' }}>
                                    <h2>Crop Your Image</h2>
                                    <ReactCrop
                                        crop={crop}
                                        onChange={(_: PixelCrop, percentCrop: Crop) => setCrop(percentCrop)}
                                        onComplete={(c: PixelCrop) => setCompletedCrop(c)}
                                        aspect={1}
                                    >
                                        <img
                                            ref={imgRef}
                                            alt="Crop me"
                                            src={imgSrc}
                                            onLoad={onImageLoad}
                                            style={{ maxHeight: '70vh' }}
                                        />
                                    </ReactCrop>
                                    <div style={{ marginTop: '15px' }}>
                                        <button onClick={handleCropConfirm} style={{ marginRight: '10px' }}>Confirm</button>
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
                                        value={formData.firstName || ''}
                                        onChange={handleChange}
                                        placeholder="First Name"
                                    />
                                    <input
                                        name="lastName"
                                        value={formData.lastName || ''}
                                        onChange={handleChange}
                                        placeholder="Last Name"
                                    />
                                    <input
                                        name="username"
                                        value={formData.username || ''}
                                        onChange={handleChange}
                                        placeholder="Username"
                                    />
                                    <input
                                        name="email"
                                        value={formData.email || ''}
                                        onChange={handleChange}
                                        placeholder="Email"
                                    />
                                    <input
                                        name="country"
                                        value={formData.country || ''}
                                        onChange={handleChange}
                                        placeholder="Country"
                                    />
                                    <input
                                        name="city"
                                        value={formData.city || ''}
                                        onChange={handleChange}
                                        placeholder="City"
                                    />
                                    <input
                                        name="streetAddress"
                                        value={formData.streetAddress || ''}
                                        onChange={handleChange}
                                        placeholder="Street Address"
                                    />
                                    <select
                                        name="gender"
                                        value={formData.gender || ''}
                                        onChange={handleChange}
                                    >
                                        <option value="">Select Gender...</option>
                                        <option value="male">Male</option>
                                        <option value="female">Female</option>
                                        <option value="other">Other</option>
                                    </select>
                                    <div style={{ display: 'flex', gap: '10px', marginTop: '10px' }}>
                                        <button type="submit">Save</button>
                                        <button type="button" onClick={handleCancel}>Cancel</button>
                                    </div>
                                </form>
                            ) : (
                                <div>
                                    <div className="user-profile-user-fullname">
                                        {formData.firstName} {formData.lastName}
                                    </div>
                                    <div className="user-profile-user-username">@{formData.username}</div>
                                    <button onClick={handleEdit}>Edit</button>
                                </div>
                            )}
                        </div>
                        <div className="user-profile-user-div">
                            <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a79ad37e4df050bd8a8095_location.png"
                                loading="lazy" alt="" className="image-5" />
                            <div>
                                {formData.country}, {formData.city}, {formData.streetAddress}
                            </div>
                        </div>
                        <div className="user-profile-user-div">
                            <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a789647d68e02a34817ecb_date.png"
                                loading="lazy" alt="" className="image-5" />
                            <div>
                                Member since {formData.createdDate ? new Date(formData.createdDate).toLocaleDateString() : ""}
                            </div>
                        </div>
                        <div className="user-profile-user-q-a-block">
                            <div className="user-profile-user-q-a-div">
                                <div className="user-profile-user-questions">{questions.length}</div>
                                <div className="text-block-12">Questions</div>
                            </div>
                            <div className="user-profile-user-q-a-div">
                                <div className="user-profile-user-answers">{formData.answersCount || 0}</div>
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
                                    <div>Answers (0)</div>
                                </div>
                            </div>
                        </div>
                        <div className="user-profile-q-a-list-div">
                            {questions.map((question) => (
                                <div key={question.rowKey} className="user-profile-q-a-item">
                                    <div className="user-profile-q-a-div" style={{ display: 'flex', alignItems: 'flex-start', width: '100%' }}>
                                        {/* Stats Div */}
                                        <div className="user-profile-q-a-div-left">
                                            <div className="user-profile-q-a-div-left-info-div">
                                                <div className="text-block-15">{question.totalVotes}</div>
                                                <div className="text-block-16">votes</div>
                                            </div>
                                            <div className="user-profile-q-a-div-left-info-div">
                                                <div className="text-block-15">0</div>
                                                <div className="text-block-16">answers</div>
                                            </div>
                                            <div className="user-profile-q-a-div-left-info-div">
                                                <div className="text-block-15">0</div>
                                                <div className="text-block-16">views</div>
                                            </div>
                                        </div>

                                        {/* Info Div */}
                                        <div className="user-profile-q-a-div-info" style={{ flex: 1, marginRight: '20px' }}>
                                            <div className="user-profile-q-a-div-info-title">
                                                {question.title}
                                            </div>
                                            <div className="user-profile-q-a-div-info-description">
                                                {question.description}
                                            </div>
                                            <div className="user-profile-q-a-div-info-date">
                                                Asked {new Date(question.timestamp).toLocaleDateString()}
                                            </div>
                                        </div>

                                        {/* Image Div */}
                                        {question.pictureUrl && (
                                            <div style={{ flexShrink: 0 }}>
                                                <img
                                                    src={question.pictureUrl}
                                                    alt="Question"
                                                    style={{
                                                        width: '100px',
                                                        height: '100px',
                                                        objectFit: 'cover',
                                                        cursor: 'pointer',
                                                        borderRadius: '5px'
                                                    }}
                                                    onClick={() => question.pictureUrl && handleQuestionImageClick(question.pictureUrl)}
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
                        position: 'fixed',
                        top: 0,
                        left: 0,
                        width: '100vw',
                        height: '100vh',
                        background: 'rgba(0,0,0,0.7)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        zIndex: 1001
                    }}
                    onClick={handleQuestionImageModalClose}
                >
                    <img
                        src={selectedQuestionImageUrl}
                        alt="Question enlarged"
                        style={{ maxWidth: '80vw', maxHeight: '80vh', borderRadius: '10px' }}
                    />
                </div>
            )}
        </section>
    );
};

export default Profile;