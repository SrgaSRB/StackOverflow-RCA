const Post = () => {

    return (
        <section className="user-profile-section">
            <div className="w-layout-blockcontainer container w-container">
                <div className="user-profile-wrapper">
                    <div className="user-profile-left-div">
                        <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a78893518a2aa043ff4749_female-placeholder.webp"
                            loading="lazy" sizes="(max-width: 767px) 100vw, (max-width: 991px) 95vw, 940.0000610351562px"
                            alt="" className="user-profile-user-image" />
                        <div>
                            <div className="user-profile-user-fullname">Srdjan Delic</div>
                            <div className="user-profile-user-username">@srdjandelic02</div>
                        </div>
                        <div className="user-profile-user-div">
                            <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a79ad37e4df050bd8a8095_location.png"
                                loading="lazy" alt="" className="image-5" />
                            <div>Republika Srbija, Vojvodina, Novi Sad, 21000</div>
                        </div>
                        <div className="user-profile-user-div">
                            <img src="https://cdn.prod.website-files.com/68a76cfd4f8cbf65b7b894b5/68a789647d68e02a34817ecb_date.png"
                                loading="lazy" alt="" className="image-5" />
                            <div>Member since June 15, 2023</div>
                        </div>
                        <div className="user-profile-user-q-a-block">
                            <div className="user-profile-user-q-a-div">
                                <div className="user-profile-user-questions">15</div>
                                <div className="text-block-12">Questions</div>
                            </div>
                            <div className="user-profile-user-q-a-div">
                                <div className="user-profile-user-answers">42</div>
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

export default Post;