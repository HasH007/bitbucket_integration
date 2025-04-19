import { useState } from 'react';
import './RepoForm.css';

const RepoForm = () => {
    const [formData, setFormData] = useState({
        repository: '',
        branchName: '',
        projectType: '',
        referredRepos: []
    });
    const [message, setMessage] = useState('');
    const [showPopup, setShowPopup] = useState(false);
    const [newReferredRepo, setNewReferredRepo] = useState({
        repository: '',
        branchName: ''
    });

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const handleReferredRepoChange = (e) => {
        const { name, value } = e.target;
        setNewReferredRepo(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const addReferredRepo = () => {
        if (newReferredRepo.repository && newReferredRepo.branchName) {
            setFormData(prev => ({
                ...prev,
                referredRepos: [...prev.referredRepos, newReferredRepo]
            }));
            setNewReferredRepo({ repository: '', branchName: '' });
            setShowPopup(false);
        }
    };

    const removeReferredRepo = (index) => {
        setFormData(prev => ({
            ...prev,
            referredRepos: prev.referredRepos.filter((_, i) => i !== index)
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const response = await fetch('/testcodegenerator/processrepo', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(formData)
            });

            const data = await response.json();
            setMessage(data.message);
        } catch (error) {
            setMessage('Error submitting form: ' + error.message);
        }
    };

    return (
        <div className="repo-form-container">
            <h2>Repository Information</h2>
            <form onSubmit={handleSubmit} className="repo-form">
                <div className="form-group">
                    <label htmlFor="repository">Primary Repository URL:</label>
                    <input
                        type="text"
                        id="repository"
                        name="repository"
                        value={formData.repository}
                        onChange={handleChange}
                        required
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="branchName">Branch Name:</label>
                    <input
                        type="text"
                        id="branchName"
                        name="branchName"
                        value={formData.branchName}
                        onChange={handleChange}
                        required
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="projectType">Project Type:</label>
                    <select
                        id="projectType"
                        name="projectType"
                        value={formData.projectType}
                        onChange={handleChange}
                        required
                    >
                        <option value="">Select a project type</option>
                        <option value="Java">Java</option>
                        <option value=".NET">.NET</option>
                    </select>
                </div>

                <div className="referred-repos-section">
                    <div className="referred-repos-header">
                        <h3>Additional Referred Repositories</h3>
                        <button
                            type="button"
                            className="add-link"
                            onClick={() => setShowPopup(true)}
                        >
                            Add Repository
                        </button>
                    </div>
                    <div className="referred-repos-list">
                        {formData.referredRepos.map((repo, index) => (
                            <div key={index} className="referred-repo-item">
                                <div className="repo-details">
                                    <div className="repo-url">{repo.repository}</div>
                                    <div className="repo-branch">Branch: {repo.branchName}</div>
                                </div>
                                <button
                                    type="button"
                                    className="remove-button"
                                    onClick={() => removeReferredRepo(index)}
                                >
                                    ×
                                </button>
                            </div>
                        ))}
                    </div>
                </div>

                <button type="submit" className="submit-button">Submit</button>
            </form>
            {message && <div className="message">{message}</div>}

            {showPopup && (
                <div className="popup-overlay">
                    <div className="popup-content">
                        <h3>Add Referred Repository</h3>
                        <div className="form-group">
                            <label htmlFor="referredRepository">Repository URL:</label>
                            <input
                                type="text"
                                id="referredRepository"
                                name="repository"
                                value={newReferredRepo.repository}
                                onChange={handleReferredRepoChange}
                                placeholder="Enter repository URL"
                            />
                        </div>
                        <div className="form-group">
                            <label htmlFor="referredBranchName">Branch Name:</label>
                            <input
                                type="text"
                                id="referredBranchName"
                                name="branchName"
                                value={newReferredRepo.branchName}
                                onChange={handleReferredRepoChange}
                                placeholder="Enter branch name"
                            />
                        </div>
                        <div className="popup-buttons">
                            <button
                                type="button"
                                className="cancel-button"
                                onClick={() => {
                                    setShowPopup(false);
                                    setNewReferredRepo({ repository: '', branchName: '' });
                                }}
                            >
                                Cancel
                            </button>
                            <button
                                type="button"
                                className="add-button"
                                onClick={addReferredRepo}
                                disabled={!newReferredRepo.repository || !newReferredRepo.branchName}
                            >
                                Add
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default RepoForm; 