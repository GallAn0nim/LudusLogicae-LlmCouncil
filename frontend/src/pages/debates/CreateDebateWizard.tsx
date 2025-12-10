import { useState, useEffect, useRef, useMemo } from 'react';
import { Check, ChevronRight, Users, HelpCircle, Settings } from 'lucide-react';
import anime from 'animejs';
import type { CreateDebateRequest, Member, Debate, MemberSortOption } from '../../types';
import { membersApi } from '../../api/client';
import { Button, Input, Textarea, Slider, Card, Select } from '../../components/ui';
import styles from './CreateDebateWizard.module.css';

interface CreateDebateWizardProps {
  onSubmit: (data: CreateDebateRequest) => void;
  onCancel: () => void;
  loading?: boolean;
  initialData?: Debate;
}

type Step = 'question' | 'settings' | 'members';

export function CreateDebateWizard({ onSubmit, onCancel, loading, initialData }: CreateDebateWizardProps) {
  const [step, setStep] = useState<Step>('question');
  const [formData, setFormData] = useState<CreateDebateRequest>({
    question: initialData?.question || '',
    context: initialData?.context || '',
    nMembers: initialData?.nMembers || 5,
    nRounds: initialData?.nRounds || 3,
    memberIds: initialData?.participants?.map(p => p.memberId) || [],
  });
  const [availableMembers, setAvailableMembers] = useState<Member[]>([]);
  const [selectedMembers, setSelectedMembers] = useState<Set<string>>(
    new Set(initialData?.participants?.map(p => p.memberId) || [])
  );
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState<MemberSortOption>('PointsDesc');
  const stepContentRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    membersApi.list({ status: 'Active', pageSize: 100, sortBy: 'PointsDesc' })
      .then((res) => setAvailableMembers(res.items))
      .catch(console.error);
  }, []);

  const filteredMembers = useMemo(() => {
    let result = availableMembers;

    if (searchTerm.trim()) {
      const term = searchTerm.toLowerCase();
      result = result.filter(m =>
        m.name.toLowerCase().includes(term) ||
        m.role.toLowerCase().includes(term)
      );
    }

    result = [...result].sort((a, b) => {
      switch (sortBy) {
        case 'PointsDesc': return b.points - a.points;
        case 'WinsDesc': return b.winsCount - a.winsCount;
        case 'DebatesDesc': return b.debatesCount - a.debatesCount;
        case 'NameAsc': return a.name.localeCompare(b.name);
        default: return 0;
      }
    });

    return result;
  }, [availableMembers, searchTerm, sortBy]);

  useEffect(() => {
    if (stepContentRef.current) {
      anime({
        targets: stepContentRef.current,
        opacity: [0, 1],
        translateX: [20, 0],
        duration: 300,
        easing: 'easeOutCubic',
      });
    }
  }, [step]);

  const steps: { key: Step; label: string; icon: React.ReactNode }[] = [
    { key: 'question', label: 'Topic', icon: <HelpCircle size={16} /> },
    { key: 'settings', label: 'Settings', icon: <Settings size={16} /> },
    { key: 'members', label: 'Members', icon: <Users size={16} /> },
  ];

  const currentStepIndex = steps.findIndex((s) => s.key === step);

  const validateStep = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (step === 'question') {
      if (!formData.question.trim()) {
        newErrors.question = 'A debate question is required';
      } else if (formData.question.length < 10) {
        newErrors.question = 'Question should be at least 10 characters';
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleNext = () => {
    if (!validateStep()) return;

    if (step === 'question') setStep('settings');
    else if (step === 'settings') setStep('members');
    else handleSubmit();
  };

  const handleBack = () => {
    if (step === 'settings') setStep('question');
    else if (step === 'members') setStep('settings');
  };

  const handleSubmit = () => {
    const submitData: CreateDebateRequest = {
      ...formData,
      memberIds: selectedMembers.size > 0 ? Array.from(selectedMembers) : undefined,
    };
    onSubmit(submitData);
  };

  const toggleMember = (memberId: string) => {
    setSelectedMembers((prev) => {
      const next = new Set(prev);
      if (next.has(memberId)) {
        next.delete(memberId);
      } else if (next.size < formData.nMembers!) {
        next.add(memberId);
      }
      return next;
    });
  };

  const renderStepContent = () => {
    switch (step) {
      case 'question':
        return (
          <div className={styles.stepContent} ref={stepContentRef}>
            <div className={styles.stepHeader}>
              <h3>What should they debate?</h3>
              <p>Enter a thought-provoking question for the AI members to discuss</p>
            </div>
            <Textarea
              value={formData.question}
              onChange={(e) => {
                setFormData({ ...formData, question: e.target.value });
                if (errors.question) setErrors({});
              }}
              placeholder="e.g., Is artificial general intelligence achievable within the next decade?"
              error={errors.question}
              rows={3}
            />
            <Input
              label="Additional Context (optional)"
              value={formData.context || ''}
              onChange={(e) => setFormData({ ...formData, context: e.target.value })}
              placeholder="Provide any background information or constraints..."
            />
          </div>
        );

      case 'settings':
        return (
          <div className={styles.stepContent} ref={stepContentRef}>
            <div className={styles.stepHeader}>
              <h3>Configure the debate</h3>
              <p>Set the number of participants and rounds</p>
            </div>
            <Slider
              label="Number of Members"
              value={formData.nMembers}
              onChange={(e) => {
                const value = parseInt(e.target.value);
                setFormData({ ...formData, nMembers: value });
                if (selectedMembers.size > value) {
                  setSelectedMembers(new Set(Array.from(selectedMembers).slice(0, value)));
                }
              }}
              min={3}
              max={10}
              step={1}
            />
            <Slider
              label="Number of Rounds"
              value={formData.nRounds}
              onChange={(e) => setFormData({ ...formData, nRounds: parseInt(e.target.value) })}
              min={1}
              max={5}
              step={1}
            />
            <div className={styles.settingsSummary}>
              <p>
                The debate will have <strong>{formData.nMembers} participants</strong> engaging in{' '}
                <strong>{formData.nRounds} round{formData.nRounds !== 1 ? 's' : ''}</strong> of
                cross-examination before voting.
              </p>
            </div>
          </div>
        );

      case 'members':
        return (
          <div className={styles.stepContent} ref={stepContentRef}>
            <div className={styles.stepHeader}>
              <h3>Select Participants</h3>
              <p>
                Choose up to {formData.nMembers} members or leave empty for random selection.
                {selectedMembers.size > 0 && (
                  <span className={styles.selectedCount}>
                    {' '}({selectedMembers.size}/{formData.nMembers} selected)
                  </span>
                )}
              </p>
            </div>
            <div className={styles.memberFilters}>
              <div className={styles.memberSearchWrapper}>
                <Input
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  placeholder="Search by name or role..."
                />
              </div>
              <Select
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value as MemberSortOption)}
                options={[
                  { value: 'PointsDesc', label: 'Most Points' },
                  { value: 'WinsDesc', label: 'Most Wins' },
                  { value: 'DebatesDesc', label: 'Most Debates' },
                  { value: 'NameAsc', label: 'Name (A-Z)' },
                ]}
              />
            </div>
            <div className={styles.memberGrid}>
              {filteredMembers.map((member) => {
                const isSelected = selectedMembers.has(member.id);
                const isDisabled = !isSelected && selectedMembers.size >= formData.nMembers!;

                return (
                  <Card
                    key={member.id}
                    className={`${styles.memberOption} ${isSelected ? styles.selected : ''} ${isDisabled ? styles.disabled : ''}`}
                    padding="sm"
                    onClick={() => !isDisabled && toggleMember(member.id)}
                  >
                    <div className={styles.memberCheck}>
                      {isSelected && <Check size={14} />}
                    </div>
                    <div className={styles.memberInfo}>
                      <span className={styles.memberName}>{member.name}</span>
                      <span className={styles.memberRole}>{member.role}</span>
                    </div>
                    <span className={styles.memberPoints}>{member.points} pts</span>
                  </Card>
                );
              })}
            </div>
            {selectedMembers.size === 0 && (
              <p className={styles.randomNote}>
                Leave unselected to randomly choose {formData.nMembers} active members
              </p>
            )}
          </div>
        );
    }
  };

  return (
    <div className={styles.wizard}>
      <div className={styles.steps}>
        {steps.map((s, i) => (
          <div
            key={s.key}
            className={`${styles.step} ${i <= currentStepIndex ? styles.active : ''} ${i < currentStepIndex ? styles.completed : ''}`}
          >
            <div className={styles.stepIcon}>
              {i < currentStepIndex ? <Check size={14} /> : s.icon}
            </div>
            <span className={styles.stepLabel}>{s.label}</span>
            {i < steps.length - 1 && <ChevronRight size={14} className={styles.stepArrow} />}
          </div>
        ))}
      </div>

      {renderStepContent()}

      <div className={styles.actions}>
        <Button variant="secondary" onClick={step === 'question' ? onCancel : handleBack}>
          {step === 'question' ? 'Cancel' : 'Back'}
        </Button>
        <Button onClick={handleNext} loading={loading && step === 'members'}>
          {step === 'members' ? (initialData ? 'Update Debate' : 'Create Debate') : 'Continue'}
        </Button>
      </div>
    </div>
  );
}
