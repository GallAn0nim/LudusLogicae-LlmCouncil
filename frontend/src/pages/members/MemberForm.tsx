import { useState, useEffect, FormEvent } from 'react';
import type { Member, CreateMemberRequest, ProviderInfo, ModelInfo, MemberStatus } from '../../types';
import { Button, Input, Textarea, Select, Slider } from '../../components/ui';
import { metadataApi } from '../../api/client';
import styles from './MemberForm.module.css';

interface MemberFormProps {
  member?: Member | null;
  onSubmit: (data: CreateMemberRequest) => void;
  onCancel: () => void;
  loading?: boolean;
}

export function MemberForm({ member, onSubmit, onCancel, loading }: MemberFormProps) {
  const [providers, setProviders] = useState<ProviderInfo[]>([]);
  const [models, setModels] = useState<ModelInfo[]>([]);
  const [loadingMetadata, setLoadingMetadata] = useState(true);
  const [formData, setFormData] = useState<CreateMemberRequest>({
    name: member?.name || '',
    role: member?.role || '',
    provider: member?.provider || '',
    model: member?.model || '',
    temperature: member?.temperature ?? 0.5,
    styleHint: member?.styleHint || '',
    behaviorExtras: member?.behaviorExtras || '',
    status: member?.status || 'Active',
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    setLoadingMetadata(true);
    metadataApi.getProviders()
      .then((data) => {
        console.log('Providers loaded:', data);
        const mappedProviders = Array.isArray(data)
          ? data.map((p: any) => typeof p === 'string' ? { id: p, name: p } : p)
          : [];
        setProviders(mappedProviders);
      })
      .catch((error) => {
        console.error('Failed to load providers:', error);
        setErrors((prev) => ({ ...prev, provider: 'Failed to load providers' }));
      })
      .finally(() => setLoadingMetadata(false));
  }, []);

  useEffect(() => {
    if (formData.provider) {
      metadataApi.getModels(formData.provider)
        .then((data) => {
          console.log('Models loaded:', data);
          const mappedModels = Array.isArray(data)
            ? data.map((m: any) => typeof m === 'string' ? { id: m, name: m } : m)
            : [];
          setModels(mappedModels);
        })
        .catch((error) => {
          console.error('Failed to load models:', error);
          setErrors((prev) => ({ ...prev, model: 'Failed to load models' }));
        });
    } else {
      setModels([]);
    }
  }, [formData.provider]);

  useEffect(() => {
    if (providers.length > 0 && !formData.provider) {
      console.log('Auto-selecting first provider:', providers[0].id);
      setFormData((prev) => ({ ...prev, provider: providers[0].id }));
    }
  }, [providers, formData.provider]);

  useEffect(() => {
    if (models.length > 0 && !formData.model) {
      console.log('Auto-selecting first model:', models[0].id);
      setFormData((prev) => ({ ...prev, model: models[0].id }));
    }
  }, [models, formData.model]);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.name.trim()) {
      newErrors.name = 'Name is required';
    } else if (formData.name.length > 200) {
      newErrors.name = 'Name must be 200 characters or less';
    }

    if (!formData.role.trim()) {
      newErrors.role = 'Role is required';
    } else if (formData.role.length > 100) {
      newErrors.role = 'Role must be 100 characters or less';
    }

    if (!formData.provider) {
      newErrors.provider = 'Provider is required';
    }

    if (!formData.model) {
      newErrors.model = 'Model is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (validate()) {
      onSubmit(formData);
    }
  };

  const handleChange = (field: keyof CreateMemberRequest, value: unknown) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: '' }));
    }
  };

  return (
    <form onSubmit={handleSubmit} className={styles.form}>
      <div className={styles.grid}>
        <Input
          label="Name"
          value={formData.name}
          onChange={(e) => handleChange('name', e.target.value)}
          placeholder="e.g., Aristotle"
          error={errors.name}
        />
        <Input
          label="Role"
          value={formData.role}
          onChange={(e) => handleChange('role', e.target.value)}
          placeholder="e.g., Philosopher"
          error={errors.role}
        />
      </div>

      <div className={styles.grid}>
        <Select
          label="Provider"
          value={formData.provider}
          onChange={(e) => handleChange('provider', e.target.value)}
          options={providers.map((p) => ({ value: p.id, label: p.name }))}
          error={errors.provider}
        />
        <Select
          label="Model"
          value={formData.model}
          onChange={(e) => handleChange('model', e.target.value)}
          options={models.map((m) => ({ value: m.id, label: m.name }))}
          error={errors.model}
        />
      </div>

      <div className={styles.sliderContainer}>
        <Slider
          label="Temperature"
          value={formData.temperature}
          onChange={(e) => handleChange('temperature', parseFloat(e.target.value))}
          min={0}
          max={1}
          step={0.1}
          formatValue={(v) => v.toFixed(1)}
        />
        <div className={styles.sliderLabels}>
          <span>deterministic</span>
          <span>balanced</span>
          <span>creative</span>
        </div>
      </div>

      <Select
        label="Status"
        value={formData.status}
        onChange={(e) => handleChange('status', e.target.value as MemberStatus)}
        options={[
          { value: 'Active', label: 'Active' },
          { value: 'Benched', label: 'Benched' },
          { value: 'Retired', label: 'Retired' },
        ]}
      />

      <Input
        label="Style Hint"
        value={formData.styleHint || ''}
        onChange={(e) => handleChange('styleHint', e.target.value)}
        placeholder="e.g., Speaks in formal philosophical language"
        hint="A brief description of how this member communicates"
      />

      <Textarea
        label="Behavior Extras"
        value={formData.behaviorExtras || ''}
        onChange={(e) => handleChange('behaviorExtras', e.target.value)}
        placeholder="Additional behavioral instructions..."
        hint="Extra instructions for the AI model"
      />

      <div className={styles.actions}>
        <Button type="button" variant="secondary" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" loading={loading}>
          {member ? 'Update Member' : 'Create Member'}
        </Button>
      </div>
    </form>
  );
}
