import { useMemo, useState } from 'react';

const TASK_STATUS = {
  Pending: 0,
  InProgress: 1,
  Completed: 2,
};

const TASK_PRIORITY = {
  Low: 0,
  Medium: 1,
  High: 2,
  Urgent: 3,
};

const STATUS_LABELS = ['Pending', 'In Progress', 'Completed'];
const PRIORITY_LABELS = ['Low', 'Medium', 'High', 'Urgent'];

const defaultFormState = {
  id: '',
  title: '',
  description: '',
  status: TASK_STATUS.Pending,
  priority: TASK_PRIORITY.Medium,
  category: 'General',
  dueDate: '',
  assignedUserId: 'user-101',
};

const initialTasks = [
  {
    id: 't-101',
    title: 'Prepare sprint backlog',
    description: 'Finalize the tasks for the upcoming sprint review and align priorities.',
    status: TASK_STATUS.Pending,
    priority: TASK_PRIORITY.High,
    category: 'Planning',
    dueDate: '2026-08-18',
    assignedUserId: 'user-101',
    createdAt: '2026-08-10T09:00:00.000Z',
  },
  {
    id: 't-102',
    title: 'Fix authentication flow',
    description: 'Resolve token refresh edge cases and update error response handling.',
    status: TASK_STATUS.InProgress,
    priority: TASK_PRIORITY.Urgent,
    category: 'Engineering',
    dueDate: '2026-08-15',
    assignedUserId: 'user-102',
    createdAt: '2026-08-11T08:30:00.000Z',
  },
  {
    id: 't-103',
    title: 'Review QA checklist',
    description: 'Confirm regression test coverage and prepare release sign-off.',
    status: TASK_STATUS.Completed,
    priority: TASK_PRIORITY.Medium,
    category: 'Quality',
    dueDate: '2026-08-12',
    assignedUserId: 'user-103',
    createdAt: '2026-08-09T15:00:00.000Z',
  },
];

const createId = () => {
  if (typeof crypto !== 'undefined' && crypto.randomUUID) {
    return crypto.randomUUID();
  }

  return `task-${Date.now()}-${Math.random().toString(16).slice(2)}`;
};

const formatDate = (dateValue) => {
  if (!dateValue) return 'No due date';

  const date = new Date(dateValue);
  if (Number.isNaN(date.getTime())) return 'No due date';

  return new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  }).format(date);
};

const getStatusLabel = (statusValue) => {
  const label = STATUS_LABELS[Number(statusValue)] ?? 'Pending';
  return label;
};

const getPriorityLabel = (priorityValue) => {
  const label = PRIORITY_LABELS[Number(priorityValue)] ?? 'Medium';
  return label;
};

const TaskDashboard = () => {
  const [tasks, setTasks] = useState(initialTasks);
  const [statusFilter, setStatusFilter] = useState('All');
  const [priorityFilter, setPriorityFilter] = useState('All');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingTask, setEditingTask] = useState(null);
  const [formData, setFormData] = useState(defaultFormState);

  const filteredTasks = useMemo(() => {
    return tasks.filter((task) => {
      const statusMatch = statusFilter === 'All' || getStatusLabel(task.status) === statusFilter;
      const priorityMatch = priorityFilter === 'All' || getPriorityLabel(task.priority) === priorityFilter;
      return statusMatch && priorityMatch;
    });
  }, [tasks, statusFilter, priorityFilter]);

  const summary = useMemo(() => {
    const counts = {
      total: tasks.length,
      pending: tasks.filter((task) => Number(task.status) === TASK_STATUS.Pending).length,
      inProgress: tasks.filter((task) => Number(task.status) === TASK_STATUS.InProgress).length,
      completed: tasks.filter((task) => Number(task.status) === TASK_STATUS.Completed).length,
      urgent: tasks.filter((task) => Number(task.priority) === TASK_PRIORITY.Urgent).length,
    };

    return counts;
  }, [tasks]);

  const openCreateModal = () => {
    setEditingTask(null);
    setFormData({
      ...defaultFormState,
      id: '',
      category: 'General',
      assignedUserId: 'user-101',
    });
    setIsModalOpen(true);
  };

  const openEditModal = (task) => {
    setEditingTask(task.id);
    setFormData({
      id: task.id,
      title: task.title,
      description: task.description || '',
      status: Number(task.status),
      priority: Number(task.priority),
      category: task.category || 'General',
      dueDate: task.dueDate || '',
      assignedUserId: task.assignedUserId || 'user-101',
    });
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setEditingTask(null);
    setFormData(defaultFormState);
  };

  const handleInputChange = (event) => {
    const { name, value } = event.target;

    setFormData((current) => ({
      ...current,
      [name]: name === 'status' || name === 'priority' ? Number(value) : value,
    }));
  };

  const handleSaveTask = (event) => {
    event.preventDefault();

    if (!formData.title.trim()) {
      return;
    }

    const taskPayload = {
      id: editingTask || createId(),
      title: formData.title.trim(),
      description: formData.description.trim(),
      status: Number(formData.status),
      priority: Number(formData.priority),
      category: formData.category.trim() || 'General',
      dueDate: formData.dueDate || null,
      assignedUserId: formData.assignedUserId || 'user-101',
      createdAt: editingTask
        ? tasks.find((task) => task.id === editingTask)?.createdAt || new Date().toISOString()
        : new Date().toISOString(),
    };

    setTasks((currentTasks) => {
      if (editingTask) {
        return currentTasks.map((task) => (task.id === editingTask ? taskPayload : task));
      }

      return [taskPayload, ...currentTasks];
    });

    closeModal();
  };

  const handleDeleteTask = (taskId) => {
    setTasks((currentTasks) => currentTasks.filter((task) => task.id !== taskId));
  };

  return (
    <>
      <style>
        {`
          * { box-sizing: border-box; }
          .task-dashboard { font-family: Arial, sans-serif; padding: 24px; background: #f4f7fb; min-height: 100vh; }
          .task-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
          .task-header h2 { margin: 0; font-size: 30px; color: #1f2937; }
          .primary-btn { background: #2563eb; color: white; border: none; padding: 10px 16px; border-radius: 10px; cursor: pointer; font-weight: 600; }
          .secondary-btn { background: #e5e7eb; color: #111827; border: none; padding: 8px 12px; border-radius: 8px; cursor: pointer; }
          .summary-grid { display: grid; grid-template-columns: repeat(5, minmax(120px, 1fr)); gap: 16px; margin-bottom: 20px; }
          .summary-card { background: white; border-radius: 14px; padding: 18px; box-shadow: 0 1px 3px rgba(15, 23, 42, 0.08); }
          .summary-label { font-size: 12px; color: #6b7280; text-transform: uppercase; letter-spacing: 0.08em; }
          .summary-value { margin-top: 10px; font-size: 28px; font-weight: 700; color: #111827; }
          .filter-bar { background: white; border-radius: 14px; padding: 16px; box-shadow: 0 1px 3px rgba(15, 23, 42, 0.08); margin-bottom: 20px; display: flex; gap: 16px; flex-wrap: wrap; align-items: center; }
          .filter-field { display: flex; flex-direction: column; gap: 6px; font-size: 12px; color: #374151; }
          .filter-field select { min-width: 140px; padding: 10px 12px; border: 1px solid #d1d5db; border-radius: 8px; background: white; }
          .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(260px, 1fr)); gap: 18px; }
          .task-card { background: white; border-radius: 14px; padding: 18px; box-shadow: 0 1px 3px rgba(15, 23, 42, 0.08); border: 1px solid #eef2f7; }
          .task-top-row { display: flex; justify-content: space-between; align-items: start; gap: 8px; }
          .task-title { font-size: 18px; font-weight: 700; margin: 0; color: #111827; }
          .pill { display: inline-flex; align-items: center; padding: 5px 10px; border-radius: 999px; font-size: 12px; font-weight: 700; }
          .status-pending { background: #fef3c7; color: #92400e; }
          .status-inprogress { background: #dbeafe; color: #1d4ed8; }
          .status-completed { background: #dcfce7; color: #166534; }
          .priority-low { background: #ecfccb; color: #3f6212; }
          .priority-medium { background: #fef3c7; color: #92400e; }
          .priority-high { background: #fee2e2; color: #b91c1c; }
          .priority-urgent { background: #fca5a5; color: #7f1d1d; }
          .task-meta { margin: 12px 0; color: #6b7280; font-size: 13px; }
          .task-description { color: #374151; line-height: 1.5; margin: 0 0 12px 0; }
          .task-footer { display: flex; justify-content: space-between; align-items: center; gap: 8px; }
          .task-actions { display: flex; gap: 8px; }
          .modal-backdrop { position: fixed; inset: 0; background: rgba(15, 23, 42, 0.48); display: flex; justify-content: center; align-items: center; padding: 16px; }
          .modal { width: min(620px, 100%); background: white; border-radius: 16px; padding: 24px; box-shadow: 0 20px 60px rgba(0,0,0,0.2); }
          .modal-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
          .modal-header h3 { margin: 0; font-size: 24px; color: #111827; }
          .close-btn { border: none; background: transparent; font-size: 26px; cursor: pointer; color: #374151; }
          .form-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; }
          .form-field { display: flex; flex-direction: column; gap: 6px; color: #374151; }
          .form-field.full-width { grid-column: 1 / -1; }
          .form-field input, .form-field textarea, .form-field select { width: 100%; padding: 10px 12px; border: 1px solid #d1d5db; border-radius: 8px; font-size: 14px; }
          .form-field textarea { min-height: 90px; resize: vertical; }
          .modal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px; }
          @media (max-width: 640px) { .summary-grid, .form-grid { grid-template-columns: 1fr; } .task-header { flex-direction: column; align-items: flex-start; gap: 12px; } }
        `}
      </style>

      <div className="task-dashboard">
        <div className="task-header">
          <h2>Task Management Dashboard</h2>
          <button className="primary-btn" type="button" onClick={openCreateModal}>
            + New Task
          </button>
        </div>

        <div className="summary-grid">
          <div className="summary-card">
            <div className="summary-label">Total</div>
            <div className="summary-value">{summary.total}</div>
          </div>
          <div className="summary-card">
            <div className="summary-label">Pending</div>
            <div className="summary-value">{summary.pending}</div>
          </div>
          <div className="summary-card">
            <div className="summary-label">In Progress</div>
            <div className="summary-value">{summary.inProgress}</div>
          </div>
          <div className="summary-card">
            <div className="summary-label">Completed</div>
            <div className="summary-value">{summary.completed}</div>
          </div>
          <div className="summary-card">
            <div className="summary-label">Urgent</div>
            <div className="summary-value">{summary.urgent}</div>
          </div>
        </div>

        <div className="filter-bar">
          <div className="filter-field">
            <label htmlFor="statusFilter">Status</label>
            <select id="statusFilter" value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}>
              <option value="All">All</option>
              {STATUS_LABELS.map((status) => (
                <option key={status} value={status}>
                  {status}
                </option>
              ))}
            </select>
          </div>

          <div className="filter-field">
            <label htmlFor="priorityFilter">Priority</label>
            <select id="priorityFilter" value={priorityFilter} onChange={(event) => setPriorityFilter(event.target.value)}>
              <option value="All">All</option>
              {PRIORITY_LABELS.map((priority) => (
                <option key={priority} value={priority}>
                  {priority}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="grid">
          {filteredTasks.map((task) => (
            <article key={task.id} className="task-card">
              <div className="task-top-row">
                <h3 className="task-title">{task.title}</h3>
                <span className={`pill status-${getStatusLabel(task.status).toLowerCase().replace(/\s+/g, '')}`}>
                  {getStatusLabel(task.status)}
                </span>
              </div>

              <div className="task-meta">
                <div><strong>Category:</strong> {task.category || 'General'}</div>
                <div><strong>Priority:</strong> <span className={`pill priority-${getPriorityLabel(task.priority).toLowerCase()}`}>{getPriorityLabel(task.priority)}</span></div>
                <div><strong>Due:</strong> {formatDate(task.dueDate)}</div>
              </div>

              <p className="task-description">{task.description || 'No description provided.'}</p>

              <div className="task-footer">
                <small>{new Date(task.createdAt).toLocaleDateString()}</small>
                <div className="task-actions">
                  <button className="secondary-btn" type="button" onClick={() => openEditModal(task)}>
                    Edit
                  </button>
                  <button className="secondary-btn" type="button" onClick={() => handleDeleteTask(task.id)}>
                    Delete
                  </button>
                </div>
              </div>
            </article>
          ))}
        </div>
      </div>

      {isModalOpen && (
        <div className="modal-backdrop" onClick={closeModal}>
          <div className="modal" onClick={(event) => event.stopPropagation()}>
            <div className="modal-header">
              <h3>{editingTask ? 'Edit Task' : 'Create Task'}</h3>
              <button type="button" className="close-btn" onClick={closeModal} aria-label="Close modal">
                ×
              </button>
            </div>

            <form onSubmit={handleSaveTask}>
              <div className="form-grid">
                <div className="form-field full-width">
                  <label htmlFor="title">Title</label>
                  <input
                    id="title"
                    name="title"
                    type="text"
                    value={formData.title}
                    onChange={handleInputChange}
                    placeholder="Task title"
                    required
                  />
                </div>

                <div className="form-field full-width">
                  <label htmlFor="description">Description</label>
                  <textarea
                    id="description"
                    name="description"
                    value={formData.description}
                    onChange={handleInputChange}
                    placeholder="Task description"
                  />
                </div>

                <div className="form-field">
                  <label htmlFor="status">Status</label>
                  <select id="status" name="status" value={formData.status} onChange={handleInputChange}>
                    {STATUS_LABELS.map((status, index) => (
                      <option key={status} value={index}>
                        {status}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="form-field">
                  <label htmlFor="priority">Priority</label>
                  <select id="priority" name="priority" value={formData.priority} onChange={handleInputChange}>
                    {PRIORITY_LABELS.map((priority, index) => (
                      <option key={priority} value={index}>
                        {priority}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="form-field">
                  <label htmlFor="category">Category</label>
                  <input
                    id="category"
                    name="category"
                    type="text"
                    value={formData.category}
                    onChange={handleInputChange}
                    placeholder="General"
                  />
                </div>

                <div className="form-field">
                  <label htmlFor="assignedUserId">Assigned User</label>
                  <input
                    id="assignedUserId"
                    name="assignedUserId"
                    type="text"
                    value={formData.assignedUserId}
                    onChange={handleInputChange}
                    placeholder="user-101"
                  />
                </div>

                <div className="form-field full-width">
                  <label htmlFor="dueDate">Due Date</label>
                  <input
                    id="dueDate"
                    name="dueDate"
                    type="date"
                    value={formData.dueDate}
                    onChange={handleInputChange}
                  />
                </div>
              </div>

              <div className="modal-actions">
                <button type="button" className="secondary-btn" onClick={closeModal}>
                  Cancel
                </button>
                <button type="submit" className="primary-btn">
                  {editingTask ? 'Save Changes' : 'Create Task'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </>
  );
};

export default TaskDashboard;
