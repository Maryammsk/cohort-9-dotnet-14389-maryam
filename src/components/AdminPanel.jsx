import { useEffect, useState } from 'react';
import apiClient from '../api/axiosClient';

const roles = ['Admin', 'Client', 'Manager'];
const emptyForm = {
  email: '',
  password: '',
  firstName: '',
  lastName: '',
  role: 'Client',
};

const getErrorMessage = (error, fallback) => error?.response?.data?.message || fallback;

const AdminPanel = () => {
  const [users, setUsers] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [formData, setFormData] = useState(emptyForm);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  const loadUsers = async () => {
    setError('');
    try {
      const response = await apiClient.get('/admin/users');
      setUsers(response.data);
    } catch (requestError) {
      setError(getErrorMessage(requestError, 'Unable to load users.'));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  const updateForm = (event) => {
    const { name, value } = event.target;
    setFormData((current) => ({ ...current, [name]: value }));
  };

  const createUser = async (event) => {
    event.preventDefault();
    setIsSaving(true);
    setError('');
    try {
      const response = await apiClient.post('/admin/users', formData);
      setUsers((current) => [...current, response.data].sort((left, right) => left.email.localeCompare(right.email)));
      setFormData(emptyForm);
      setIsModalOpen(false);
    } catch (requestError) {
      setError(getErrorMessage(requestError, 'Unable to create the account.'));
    } finally {
      setIsSaving(false);
    }
  };

  const updateUser = async (userId, change, fallback) => {
    setError('');
    try {
      const response = await change();
      setUsers((current) => current.map((user) => (user.id === userId ? response.data : user)));
    } catch (requestError) {
      setError(getErrorMessage(requestError, fallback));
    }
  };

  return (
    <section className="admin-panel" aria-labelledby="admin-panel-title">
      <div className="admin-panel-header">
        <div>
          <p className="admin-eyebrow">Administration</p>
          <h2 id="admin-panel-title">Client Management</h2>
        </div>
        <button className="primary-btn" type="button" onClick={() => setIsModalOpen(true)}>
          + Invite account
        </button>
      </div>
      {error && <div className="admin-error" role="alert">{error}</div>}
      {isLoading ? (
        <p>Loading users...</p>
      ) : (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr><th>User</th><th>Role</th><th>Access</th><th>Actions</th></tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td><strong>{user.firstName || user.lastName ? `${user.firstName || ''} ${user.lastName || ''}`.trim() : user.email}</strong><small>{user.email}</small></td>
                  <td>
                    <select value={user.role} onChange={(event) => updateUser(user.id, () => apiClient.put(`/admin/users/${user.id}/role`, { role: event.target.value }), 'Unable to update the role.')}>
                      {roles.map((role) => <option key={role} value={role}>{role}</option>)}
                    </select>
                  </td>
                  <td><span className={`access-badge ${user.isActive ? 'access-active' : 'access-inactive'}`}>{user.isActive ? 'Active' : 'Inactive'}</span></td>
                  <td>
                    <button className="secondary-btn" type="button" onClick={() => updateUser(user.id, () => apiClient.patch(`/admin/users/${user.id}/access`, { isActive: !user.isActive }), 'Unable to update account access.')}>{user.isActive ? 'Revoke access' : 'Grant access'}</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {!users.length && <p>No users found.</p>}
        </div>
      )}

      {isModalOpen && (
        <dialog
          open
          className="modal-backdrop"
          onClick={(event) => {
            if (event.target === event.currentTarget) {
              setIsModalOpen(false);
            }
          }}
        >
          <div className="modal">
            <div className="modal-header"><h3>Invite account</h3><button type="button" className="close-btn" onClick={() => setIsModalOpen(false)} aria-label="Close modal">×</button></div>
            <form onSubmit={createUser}>
              <div className="form-grid">
                {['firstName', 'lastName', 'email', 'password'].map((field) => (
                  <div className={`form-field ${field === 'email' || field === 'password' ? 'full-width' : ''}`} key={field}>
                    <label htmlFor={`admin-${field}`}>{field === 'firstName' ? 'First name' : field === 'lastName' ? 'Last name' : field === 'email' ? 'Email' : 'Temporary password'}</label>
                    <input id={`admin-${field}`} name={field} type={field === 'password' ? 'password' : field === 'email' ? 'email' : 'text'} value={formData[field]} onChange={updateForm} required={field === 'email' || field === 'password'} />
                  </div>
                ))}
                <div className="form-field full-width"><label htmlFor="admin-role">Role</label><select id="admin-role" name="role" value={formData.role} onChange={updateForm}>{roles.map((role) => <option key={role}>{role}</option>)}</select></div>
              </div>
              <div className="modal-actions"><button type="button" className="secondary-btn" onClick={() => setIsModalOpen(false)}>Cancel</button><button type="submit" className="primary-btn" disabled={isSaving}>{isSaving ? 'Creating...' : 'Create account'}</button></div>
            </form>
          </div>
        </dialog>
      )}
    </section>
  );
};

export default AdminPanel;
