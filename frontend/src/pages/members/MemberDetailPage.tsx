import { useEffect, useRef, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Trophy, MessageSquare, TrendingDown, Star, Cpu, Info, Edit, Trash2, MessageCircle } from 'lucide-react';
import anime from 'animejs';
import { membersApi } from '../../api/client';
import type { CreateMemberRequest } from '../../types';
import { useApi, useApiMutation } from '../../hooks/useApi';
import { Button, Card, Loading, Modal } from '../../components/ui';
import { PageHeader } from '../../components/layout';
import { MemberForm } from './MemberForm';
import styles from './MemberDetailPage.module.css';

export function MemberDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const contentRef = useRef<HTMLDivElement>(null);

    const [isEditOpen, setIsEditOpen] = useState(false);
    const [isDeleteOpen, setIsDeleteOpen] = useState(false);
    const [isIntroOpen, setIsIntroOpen] = useState(false);
    const [introText, setIntroText] = useState('');

    const { data: member, loading, error, execute: fetchMember } = useApi(membersApi.get);

    const { execute: updateMember, loading: updating } = useApiMutation(
        (args: { id: string; data: CreateMemberRequest }) => membersApi.update(args.id, args.data),
        {
            onSuccess: () => {
                setIsEditOpen(false);
                fetchMember(id!);
            },
        }
    );

    const { execute: deleteMember, loading: deleting } = useApiMutation(membersApi.delete, {
        onSuccess: () => {
            navigate('/members');
        },
    });

    const { execute: getIntroduction, loading: introspecting } = useApiMutation(membersApi.getIntroduction, {
        onSuccess: (data) => {
            setIntroText(data.introduction);
            setIsIntroOpen(true);
        },
    });

    useEffect(() => {
        if (id) {
            fetchMember(id);
        }
    }, [id]);

    useEffect(() => {
        if (contentRef.current && member) {
            anime({
                targets: contentRef.current.children,
                opacity: [0, 1],
                translateY: [20, 0],
                duration: 400,
                delay: anime.stagger(100),
                easing: 'easeOutCubic',
            });
        }
    }, [member]);

    const formatDate = (dateString: string | null) => {
        if (!dateString) return 'Never';
        return new Date(dateString).toLocaleDateString('en-US', {
            month: 'long',
            day: 'numeric',
            year: 'numeric',
        });
    };

    if (loading && !member) {
        return (
            <div className={styles.loadingWrapper}>
                <Loading size="lg" text="Loading member profile..." />
            </div>
        );
    }

    if (error || !member) {
        return (
            <div className={styles.errorWrapper}>
                <p>Failed to load member: {error || 'Not found'}</p>
                <Button onClick={() => navigate('/members')}>Back to Members</Button>
            </div>
        );
    }

    const getStatusBadgeClass = (status: string): string => {
        switch (status.toLowerCase()) {
            case 'active':
                return styles.statusActive;
            case 'benched':
                return styles.statusBenched;
            case 'retired':
                return styles.statusRetired;
            default:
                return '';
        }
    };

    return (
        <div className={styles.page}>
            <div className={styles.topNav}>
                <div className={styles.actionButtons}>
                    <Button variant="danger" size="md" icon={<Trash2 size={18} />} onClick={() => setIsDeleteOpen(true)}>
                        Delete
                    </Button>
                    <Button variant="secondary" size="md" icon={<MessageCircle size={18} />} onClick={() => member && getIntroduction(member.id)} loading={introspecting}>
                        Introduce
                    </Button>
                    <Button variant="secondary" size="md" icon={<Edit size={18} />} onClick={() => setIsEditOpen(true)}>
                        Edit
                    </Button>
                </div>
            </div>

            <PageHeader
                title={member.name}
                description={member.role}
            />

            <div ref={contentRef} className={styles.content}>
                <div className={styles.mainColumn}>
                    <Card className={styles.headerCard} padding="lg">
                        <div className={styles.headerContent}>
                            <div className={styles.profileHeader}>
                                <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', width: '100%' }}>
                                    <span className={`${styles.statusBadge} ${getStatusBadgeClass(member.status)}`}>
                                        {member.status.charAt(0).toUpperCase() + member.status.slice(1).toLowerCase()}
                                    </span>
                                </div>
                            </div>

                            <div className={styles.statsGrid}>
                                <div className={styles.statItem}>
                                    <Trophy className={styles.statIcon} />
                                    <span className={styles.statValue}>{member.winsCount}</span>
                                    <span className={styles.statLabel}>Wins</span>
                                </div>
                                <div className={styles.statItem}>
                                    <Star className={styles.statIcon} />
                                    <span className={styles.statValue}>{member.points}</span>
                                    <span className={styles.statLabel}>Points</span>
                                </div>
                                <div className={styles.statItem}>
                                    <MessageSquare className={styles.statIcon} />
                                    <span className={styles.statValue}>{member.debatesCount}</span>
                                    <span className={styles.statLabel}>Debates</span>
                                </div>
                                <div className={styles.statItem}>
                                    <TrendingDown className={styles.statIcon} />
                                    <span className={styles.statValue}>{member.lastPlaceCount}</span>
                                    <span className={styles.statLabel}>Last Place</span>
                                </div>
                            </div>
                        </div>
                    </Card>

                    <Card className={styles.section} padding="lg">
                        <div className={styles.sectionTitle}>
                            <Info size={20} />
                            <h2>About & Style</h2>
                        </div>
                        {member.styleHint ? (
                            <p className={styles.styleHint}>"{member.styleHint}"</p>
                        ) : (
                            <p className={styles.styleHint}>No specific style instructions provided.</p>
                        )}
                    </Card>
                </div>

                <div className={styles.sideColumn}>
                    <Card className={styles.section} padding="lg">
                        <div className={styles.sectionTitle}>
                            <Cpu size={20} />
                            <h2>Configuration</h2>
                        </div>

                        <div className={styles.infoItem}>
                            <span className={styles.infoLabel}>AI Model</span>
                            <span className={styles.infoValue}>{member.model}</span>
                        </div>

                        <div className={styles.infoItem}>
                            <span className={styles.infoLabel}>Provider</span>
                            <span className={styles.infoValue}>{member.provider}</span>
                        </div>

                        <div className={styles.infoItem}>
                            <span className={styles.infoLabel}>Created</span>
                            <span className={styles.infoValue}>{formatDate(member.createdAt)}</span>
                        </div>

                        <div className={styles.infoItem}>
                            <span className={styles.infoLabel}>Last Debate</span>
                            <span className={styles.infoValue}>{formatDate(member.lastDebateDate)}</span>
                        </div>
                    </Card>
                </div>
            </div>

            <Modal
                isOpen={isEditOpen}
                onClose={() => setIsEditOpen(false)}
                title="Edit Member"
                size="lg"
            >
                {member && (
                    <MemberForm
                        member={member}
                        onSubmit={(data) => updateMember({ id: member.id, data })}
                        onCancel={() => setIsEditOpen(false)}
                        loading={updating}
                    />
                )}
            </Modal>

            <Modal
                isOpen={isDeleteOpen}
                onClose={() => setIsDeleteOpen(false)}
                title="Delete Member"
            >
                <div className={styles.deleteConfirm}>
                    <p>
                        Are you sure you want to delete <strong>{member?.name}</strong>? This action
                        cannot be undone.
                    </p>
                    <div className={styles.deleteActions} style={{ display: 'flex', justifyContent: 'flex-end', gap: '1rem', marginTop: '1.5rem' }}>
                        <Button variant="secondary" onClick={() => setIsDeleteOpen(false)}>
                            Cancel
                        </Button>
                        <Button variant="danger" onClick={() => member && deleteMember(member.id)} loading={deleting}>
                            Delete
                        </Button>
                    </div>
                </div>
            </Modal>

            <Modal
                isOpen={isIntroOpen}
                onClose={() => setIsIntroOpen(false)}
                title={`Introduction: ${member?.name}`}
            >
                <div className={styles.introContent}>
                    <p style={{ whiteSpace: 'pre-wrap', lineHeight: '1.6' }}>{introText}</p>
                    {introspecting && <Loading text="Generating introduction..." />}
                    {!introspecting && introText && (
                        <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: '1.5rem' }}>
                            <Button onClick={() => setIsIntroOpen(false)}>Close</Button>
                        </div>
                    )}
                </div>
            </Modal>
        </div>
    );
}
