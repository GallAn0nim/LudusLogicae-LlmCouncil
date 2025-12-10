import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Plus, Users, Search, Filter } from 'lucide-react';
import type { Member, CreateMemberRequest, MemberListQuery, MemberStatus, MemberSortOption } from '../../types';
import { membersApi } from '../../api/client';
import { useApi, useApiMutation } from '../../hooks/useApi';
import { PageHeader } from '../../components/layout';
import { Button, Modal, Loading, EmptyState, Input, Select } from '../../components/ui';
import { MemberCard } from './MemberCard';
import { MemberForm } from './MemberForm';
import styles from './MembersPage.module.css';

export function MembersPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [query, setQuery] = useState<MemberListQuery>({
    page: 1,
    pageSize: 12,
    sortBy: 'PointsDesc',
    status: (searchParams.get('status') as MemberStatus) || undefined,
  });
  const [searchTerm, setSearchTerm] = useState('');
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingMember, setEditingMember] = useState<Member | null>(null);
  const [deletingMember, setDeletingMember] = useState<Member | null>(null);
  const [introducingMember, setIntroducingMember] = useState<Member | null>(null);
  const [introText, setIntroText] = useState('');

  const { data, loading, error, execute: fetchMembers } = useApi(membersApi.list);
  const { execute: createMember, loading: creating } = useApiMutation(membersApi.create, {
    onSuccess: () => {
      setIsFormOpen(false);
      fetchMembers(query);
    },
  });
  const { execute: updateMember, loading: updating } = useApiMutation(
    (args: { id: string; data: CreateMemberRequest }) => membersApi.update(args.id, args.data),
    {
      onSuccess: () => {
        setEditingMember(null);
        fetchMembers(query);
      },
    }
  );
  const { execute: deleteMember, loading: deleting } = useApiMutation(membersApi.delete, {
    onSuccess: () => {
      setDeletingMember(null);
      fetchMembers(query);
    },
  });

  const { execute: getIntroduction, loading: introspecting } = useApiMutation(membersApi.getIntroduction, {
    onSuccess: (data) => {
      setIntroText(data.introduction);
    },
  });

  useEffect(() => {
    fetchMembers(query);
  }, [query]);

  const handleSearch = useCallback(() => {
    setQuery((prev) => ({ ...prev, searchTerm: searchTerm || undefined, page: 1 }));
  }, [searchTerm]);

  // Auto-search after 3 characters with 1s debounce
  useEffect(() => {
    if (searchTerm.length >= 3) {
      const timer = setTimeout(() => {
        handleSearch();
      }, 1000);
      return () => clearTimeout(timer);
    }
  }, [searchTerm, handleSearch]);

  const handleFilterChange = (field: keyof MemberListQuery, value: unknown) => {
    const newQuery = { ...query, [field]: value || undefined, page: 1 };
    setQuery(newQuery);

    // Update URL params
    if (field === 'status') {
      if (value) {
        searchParams.set('status', value as string);
      } else {
        searchParams.delete('status');
      }
      setSearchParams(searchParams);
    }
  };

  const handleCreateSubmit = (formData: CreateMemberRequest) => {
    createMember(formData);
  };

  const handleEditSubmit = (formData: CreateMemberRequest) => {
    if (editingMember) {
      updateMember({ id: editingMember.id, data: formData });
    }
  };

  const handleDeleteConfirm = () => {
    if (deletingMember) {
      deleteMember(deletingMember.id);
    }
  };

  return (
    <div className={styles.page}>
      <PageHeader
        title="Members"
        description="Manage AI debate participants with unique personalities and configurations"
        action={
          <Button icon={<Plus size={18} />} onClick={() => setIsFormOpen(true)}>
            New Member
          </Button>
        }
      />

      <div className={styles.filters}>
        <div className={styles.searchWrapper}>
          <Input
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            placeholder="Search members..."
          />
          <Button variant="secondary" icon={<Search size={16} />} onClick={handleSearch}>
            Search
          </Button>
        </div>

        <div className={styles.filterGroup}>
          <Select
            value={query.status || ''}
            onChange={(e) => handleFilterChange('status', e.target.value as MemberStatus)}
            options={[
              { value: '', label: 'All Status' },
              { value: 'Active', label: 'Active' },
              { value: 'Benched', label: 'Benched' },
              { value: 'Retired', label: 'Retired' },
            ]}
          />
          <Select
            value={query.sortBy || 'PointsDesc'}
            onChange={(e) => handleFilterChange('sortBy', e.target.value as MemberSortOption)}
            options={[
              { value: 'PointsDesc', label: 'Most Points' },
              { value: 'WinsDesc', label: 'Most Wins' },
              { value: 'DebatesDesc', label: 'Most Debates' },
              { value: 'CreatedDesc', label: 'Newest' },
              { value: 'NameAsc', label: 'Name (A-Z)' },
            ]}
          />
        </div>
      </div>

      {loading && !data && (
        <div className={styles.loadingWrapper}>
          <Loading size="lg" text="Loading members..." />
        </div>
      )}

      {error && (
        <div className={styles.errorWrapper}>
          <p className={styles.errorText}>Failed to load members: {error}</p>
          <Button onClick={() => fetchMembers(query)}>Retry</Button>
        </div>
      )}

      {data && data.items.length === 0 && (
        <EmptyState
          icon={<Users />}
          title="No members found"
          description={query.searchTerm ? 'Try adjusting your search criteria' : 'Create your first AI debate member'}
          action={
            <Button icon={<Plus size={18} />} onClick={() => setIsFormOpen(true)}>
              Create Member
            </Button>
          }
        />
      )}

      {data && data.items.length > 0 && (
        <>
          <div className={styles.grid}>
            {data.items.map((member, index) => (
              <MemberCard
                key={member.id}
                member={member}
                index={index}
                onEdit={setEditingMember}
                onDelete={setDeletingMember}
                onIntroduce={(m) => {
                  setIntroducingMember(m);
                  setIntroText('');
                  getIntroduction(m.id);
                }}
              />
            ))}
          </div>

          {data.total > data.pageSize && (
            <div className={styles.pagination}>
              <Button
                variant="secondary"
                disabled={query.page === 1}
                onClick={() => setQuery((prev) => ({ ...prev, page: (prev.page || 1) - 1 }))}
              >
                Previous
              </Button>
              <span className={styles.pageInfo}>
                Page {query.page} of {Math.ceil(data.total / data.pageSize)}
              </span>
              <Button
                variant="secondary"
                disabled={(query.page || 1) * data.pageSize >= data.total}
                onClick={() => setQuery((prev) => ({ ...prev, page: (prev.page || 1) + 1 }))}
              >
                Next
              </Button>
            </div>
          )}
        </>
      )}

      <Modal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        title="Create New Member"
        size="lg"
      >
        <MemberForm
          onSubmit={handleCreateSubmit}
          onCancel={() => setIsFormOpen(false)}
          loading={creating}
        />
      </Modal>

      <Modal
        isOpen={!!editingMember}
        onClose={() => setEditingMember(null)}
        title="Edit Member"
        size="lg"
      >
        {editingMember && (
          <MemberForm
            member={editingMember}
            onSubmit={handleEditSubmit}
            onCancel={() => setEditingMember(null)}
            loading={updating}
          />
        )}
      </Modal>

      <Modal
        isOpen={!!deletingMember}
        onClose={() => setDeletingMember(null)}
        title="Delete Member"
      >
        <div className={styles.deleteConfirm}>
          <p>
            Are you sure you want to delete <strong>{deletingMember?.name}</strong>? This action
            cannot be undone.
          </p>
          <div className={styles.deleteActions}>
            <Button variant="secondary" onClick={() => setDeletingMember(null)}>
              Cancel
            </Button>
            <Button variant="danger" onClick={handleDeleteConfirm} loading={deleting}>
              Delete
            </Button>
          </div>
        </div>
      </Modal>

      <Modal
        isOpen={!!introducingMember}
        onClose={() => setIntroducingMember(null)}
        title={`Introduction: ${introducingMember?.name}`}
      >
        <div className={styles.deleteConfirm}> {/* Reusing container for simplicity or create styles.introContent if defined. MemberDetailPage used introContent. MembersPage doesn't have it yet. I'll rely on inline styles for now to avoid CSS edits. */}
          {introspecting && <Loading text="Generating introduction..." />}
          {!introspecting && introText && (
            <div style={{ padding: '1rem 0' }}>
              <p style={{ whiteSpace: 'pre-wrap', lineHeight: '1.6', marginBottom: '1.5rem' }}>{introText}</p>
              <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
                <Button onClick={() => setIntroducingMember(null)}>Close</Button>
              </div>
            </div>
          )}
        </div>
      </Modal>
    </div>
  );
}
