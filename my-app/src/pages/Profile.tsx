import { useState, useRef, useEffect } from 'react';

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
    const [imageFile, setImageFile] = useState<File | null>(null);
    const [imagePreview, setImagePreview] = useState<string | null>(null);

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
            // Ako postoji nova slika za upload
            if (imageFile) {
                const formDataImg = new FormData();
                formDataImg.append('file', imageFile);

                const res = await fetch(`http://localhost:5167/api/users/${userId}/profile-picture`, {
                    method: 'POST',
                    body: formDataImg
                });

                if (res.ok) {
                    const data = await res.json();
                    updatedUserData.profilePictureUrl = data.imageUrl;
                } else {
                    alert('Greška pri upload-u slike');
                    return; // Prekini ako upload slike ne uspe
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
                setImageFile(null); // Resetuj fajl slike
                setImagePreview(null); // Resetuj pregled slike
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
        setImageFile(null);
        setImagePreview(null);
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
        if (e.target.files && e.target.files[0]) {
            const file = e.target.files[0];
            setImageFile(file);
            setImagePreview(URL.createObjectURL(file));
        }
    };

    const handleModalClose = () => setShowImageModal(false);

    return (
        <section className="user-profile-section">
            <div className="w-layout-blockcontainer container w-container">
                <div className="user-profile-wrapper">
                    <div className="user-profile-left-div">
                        <img
                            src={
                                imagePreview ||
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
                                <div className="user-profile-user-questions">{formData.questionsCount || 0}</div>
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
                                    <div>Questions (1)</div>
                                </div>
                                <div className="user-profile-main-q-a-tab-nav">
                                    <div>Answers (0)</div>
                                </div>
                            </div>
                        </div>
                        <div className="user-profile-q-a-list-div">
                            <div className="user-profile-q-a-div">
                                <div className="user-profile-q-a-div-left">
                                    <div className="user-profile-q-a-div-left-info-div">
                                        <div className="text-block-15">12</div>
                                        <div className="text-block-16">votes</div>
                                    </div>
                                    <div className="user-profile-q-a-div-left-info-div">
                                        <div className="text-block-15">5</div>
                                        <div className="text-block-16">answers</div>
                                    </div>
                                    <div className="user-profile-q-a-div-left-info-div">
                                        <div className="text-block-15">340</div>
                                        <div className="text-block-16">views</div>
                                    </div>
                                </div>
                                <div className="user-profile-q-a-div-info">
                                    <div className="user-profile-q-a-div-info-title">How to implement authentication in React
                                        with TypeScript?</div>
                                    <div className="user-profile-q-a-div-info-description">Contrary to popular belief, Lorem
                                        Ipsum is not simply random text. It has roots in a piece of classical Latin
                                        literature from 45 BC, making it over 2000 years old. Richard McClintock, a Latin
                                        professor at Hampden-Sydney College in Virginia, looked up one of the more obscure
                                        Latin words, consectetur, from a L....</div>
                                    <div className="user-profile-q-a-div-info-date">Asked January 15, 2024</div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    );
};

export default Profile;