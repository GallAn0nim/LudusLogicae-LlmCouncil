import { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Plus, MessageSquare, Filter } from 'lucide-react';
import type { Debate, CreateDebateRequest, DebateListQuery, DebatePhase } from '../../types';
import { debatesApi } from '../../api/client';
import { useApi, useApiMutation } from '../../hooks/useApi';
import { PageHeader } from '../../components/layout';
import { Button, Modal, Loading, EmptyState, Select, DebateLoadingOverlay } from '../../components/ui';
import { DebateCard } from './DebateCard';
import { CreateDebateWizard } from './CreateDebateWizard';
import styles from './DebatesPage.module.css';

export function DebatesPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();
  const [query, setQuery] = useState<DebateListQuery>({
    page: 1,
    pageSize: 12,
    status: (searchParams.get('status') as DebatePhase) || undefined,
  });
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [deletingDebate, setDeletingDebate] = useState<Debate | null>(null);
  const [editingDebate, setEditingDebate] = useState<Debate | null>(null);
  const [runningDebateId, setRunningDebateId] = useState<string | null>(null);

  const { data, loading, error, execute: fetchDebates } = useApi(debatesApi.list);
  const { execute: createDebate, loading: creating } = useApiMutation(debatesApi.create, {
    onSuccess: () => {
      setIsCreateOpen(false);
      fetchDebates(query);
    },
  });
  const { execute: updateDebate, loading: updating } = useApiMutation(debatesApi.update, {
    onSuccess: () => {
      setEditingDebate(null);
      fetchDebates(query);
    },
  });
  const { execute: runDebate } = useApiMutation(debatesApi.run, {
    onSuccess: (debate) => {
      setRunningDebateId(null);
      if (debate && debate.id) {
        navigate(`/debates/${debate.id}`);
      } else {
        fetchDebates(query);
      }
    },
    onError: () => {
      setRunningDebateId(null);
    },
  });
  const { execute: deleteDebate, loading: deleting } = useApiMutation(debatesApi.delete, {
    onSuccess: () => {
      setDeletingDebate(null);
      fetchDebates(query);
    },
  });

  useEffect(() => {
    fetchDebates(query);
  }, [query]);

  const handleFilterChange = (field: keyof DebateListQuery, value: unknown) => {
    const newQuery = { ...query, [field]: value || undefined, page: 1 };
    setQuery(newQuery);

    if (field === 'status') {
      if (value) {
        searchParams.set('status', value as string);
      } else {
        searchParams.delete('status');
      }
      setSearchParams(searchParams);
    }
  };

  const handleCreateSubmit = (formData: CreateDebateRequest) => {
    createDebate(formData);
  };

  const handleRun = (debate: Debate) => {
    setRunningDebateId(debate.id);
    runDebate(debate.id);
  };

  const handleDeleteConfirm = () => {
    if (deletingDebate) {
      deleteDebate(deletingDebate.id);
    }
  };

  return (
    <div className={styles.page}>
      {runningDebateId && <DebateLoadingOverlay />}

      <PageHeader
        title="Debates"
        description="Create and manage AI-powered debates with multi-phase discussions"
        action={
          <Button icon={<Plus size={18} />} onClick={() => setIsCreateOpen(true)}>
            New Debate
          </Button>
        }
      />

      <div className={styles.filters}>
        <Select
          value={query.status || ''}
          onChange={(e) => handleFilterChange('status', e.target.value as DebatePhase)}
          options={[
            { value: '', label: 'All Status' },
            { value: 'Created', label: 'Created' },
            { value: 'Completed', label: 'Completed' },
            { value: 'Failed', label: 'Failed' },
          ]}
        />
      </div>

      {loading && !data && (
        <div className={styles.loadingWrapper}>
          <Loading size="lg" text="Loading debates..." />
        </div>
      )}

      {error && (
        <div className={styles.errorWrapper}>
          <p className={styles.errorText}>Failed to load debates: {error}</p>
          <Button onClick={() => fetchDebates(query)}>Retry</Button>
        </div>
      )}

      {data && data.length === 0 && (
        <EmptyState
          icon={<MessageSquare />}
          title="No debates found"
          description={query.status ? 'Try adjusting your filters' : 'Create your first AI debate'}
          action={
            <Button icon={<Plus size={18} />} onClick={() => setIsCreateOpen(true)}>
              Create Debate
            </Button>
          }
        />
      )}

      {data && data.length > 0 && (
        <>
          <div className={styles.grid}>
            {data.map((debate, index) => (
              <DebateCard
                key={debate.id}
                debate={debate}
                index={index}
                onRun={handleRun}
                onDelete={setDeletingDebate}
                onEdit={setEditingDebate}
              />
            ))}
          </div>

          {(data.length === (query.pageSize || 12) || (query.page || 1) > 1) && (
            <div className={styles.pagination}>
              <Button
                variant="secondary"
                disabled={query.page === 1}
                onClick={() => setQuery((prev) => ({ ...prev, page: (prev.page || 1) - 1 }))}
              >
                Previous
              </Button>
              <span className={styles.pageInfo}>
                Page {query.page}
              </span>
              <Button
                variant="secondary"
                disabled={data.length < (query.pageSize || 12)}
                onClick={() => setQuery((prev) => ({ ...prev, page: (prev.page || 1) + 1 }))}
              >
                Next
              </Button>
            </div>
          )}
        </>
      )}

      <Modal
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        title="Create New Debate"
        size="lg"
      >
        <CreateDebateWizard
          onSubmit={handleCreateSubmit}
          onCancel={() => setIsCreateOpen(false)}
          loading={creating}
        />
      </Modal>

      <Modal
        isOpen={!!editingDebate}
        onClose={() => setEditingDebate(null)}
        title="Edit Debate"
        size="lg"
      >
        {editingDebate && (
          <CreateDebateWizard
            initialData={editingDebate}
            onSubmit={(formData) => updateDebate(editingDebate.id, formData)}
            onCancel={() => setEditingDebate(null)}
            loading={updating}
          />
        )}
      </Modal>

      <Modal
        isOpen={!!deletingDebate}
        onClose={() => setDeletingDebate(null)}
        title="Delete Debate"
      >
        <div className={styles.deleteConfirm}>
          <p>
            Are you sure you want to delete this debate? This action cannot be undone.
          </p>
          <p className={styles.deleteQuestion}>
            "{deletingDebate?.question}"
          </p>
          <div className={styles.deleteActions}>
            <Button variant="secondary" onClick={() => setDeletingDebate(null)}>
              Cancel
            </Button>
            <Button variant="danger" onClick={handleDeleteConfirm} loading={deleting}>
              Delete
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
