import { useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Trophy, MessageSquare, TrendingDown, Calendar, Edit, Trash2, MessageCircle } from 'lucide-react';
import anime from 'animejs';
import type { Member } from '../../types';
import { Card, Button } from '../../components/ui';
import styles from './MemberCard.module.css';

interface MemberCardProps {
  member: Member;
  onEdit: (member: Member) => void;
  onDelete: (member: Member) => void;
  onIntroduce: (member: Member) => void;
  index?: number;
}

export function MemberCard({ member, onEdit, onDelete, onIntroduce, index = 0 }: MemberCardProps) {
  const cardRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();

  useEffect(() => {
    if (cardRef.current) {
      anime({
        targets: cardRef.current,
        opacity: [0, 1],
        translateY: [30, 0],
        scale: [0.95, 1],
        duration: 500,
        delay: index * 60,
        easing: 'easeOutCubic',
      });
    }
  }, [index]);

  const formatDate = (dateString: string | null) => {
    if (!dateString) {
      return 'Never';
    }
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const getStatusClass = (status: string) => {
    switch (status.toLowerCase()) {
      case 'active': return styles.statusActive;
      case 'benched': return styles.statusBenched;
      case 'retired': return styles.statusRetired;
      default: return '';
    }
  };

  const getCardStatusClass = (status: string) => {
    switch (status.toLowerCase()) {
      case 'active': return styles.cardActive;
      case 'benched': return styles.cardBenched;
      case 'retired': return styles.cardRetired;
      default: return '';
    }
  };

  return (
    <Card
      ref={cardRef}
      className={`${styles.card} ${getCardStatusClass(member.status)}`}
      style={{ opacity: 0 }}
      variant="interactive"
      onClick={() => navigate(`/members/${member.id}`)}
    >
      <div className={styles.header}>
        <div className={styles.info}>
          <h3 className={styles.name}>{member.name}</h3>
          <p className={styles.role}>{member.role}</p>
        </div>
        <span className={`${styles.statusBadge} ${getStatusClass(member.status)}`}>
          {member.status.charAt(0).toUpperCase() + member.status.slice(1).toLowerCase()}
        </span>
      </div>

      {member.styleHint && (
        <p className={styles.styleHint}>{member.styleHint}</p>
      )}

      <div className={styles.stats}>
        <div className={styles.stat}>
          <Trophy size={14} className={styles.statIcon} />
          <span className={styles.statValue}>{member.winsCount}</span>
          <span className={styles.statLabel}>Wins</span>
        </div>
        <div className={styles.stat}>
          <MessageSquare size={14} className={styles.statIcon} />
          <span className={styles.statValue}>{member.debatesCount}</span>
          <span className={styles.statLabel}>Debates</span>
        </div>
        <div className={styles.stat}>
          <TrendingDown size={14} className={styles.statIcon} />
          <span className={styles.statValue}>{member.lastPlaceCount}</span>
          <span className={styles.statLabel}>Last</span>
        </div>
      </div>

      <div className={styles.meta}>
        <div className={styles.model}>
          <span className={styles.metaLabel}>Model</span>
          <span className={styles.metaValue}>{member.model}</span>
        </div>
        <div className={styles.points}>
          <span className={styles.pointsValue}>{member.points}</span>
          <span className={styles.pointsLabel}>pts</span>
        </div>
      </div>

      <div className={styles.footer}>
        <div className={styles.lastDebate}>
          <Calendar size={12} />
          <span>{formatDate(member.lastDebateDate)}</span>
        </div>
        <div
          className={styles.actions}
          onClick={(e) => e.stopPropagation()}
        >
          <Button
            variant="ghost"
            size="sm"
            icon={<MessageCircle size={14} />}
            onClick={() => onIntroduce(member)}
            aria-label="Introduce member"
          />
          <Button
            variant="ghost"
            size="sm"
            icon={<Edit size={14} />}
            onClick={() => onEdit(member)}
            aria-label="Edit member"
          />
          <Button
            variant="ghost"
            size="sm"
            icon={<Trash2 size={14} />}
            onClick={() => onDelete(member)}
            aria-label="Delete member"
          />
        </div>
      </div>
    </Card>
  );
}
