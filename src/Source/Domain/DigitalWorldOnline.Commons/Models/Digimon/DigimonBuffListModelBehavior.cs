namespace DigitalWorldOnline.Commons.Models.Digimon
{
    public sealed partial class DigimonBuffListModel
    {
        /// <summary>
        /// Returns true if the digimon haves any active buff.
        /// </summary>
        public bool HasActiveBuffs => ActiveBuffs.Any(x => x.Duration > 0);

        /// <summary>
        /// Returns digimon's active buffs list.
        /// </summary>
        public List<DigimonBuffModel> ActiveBuffs => Buffs.Where(x => x.BuffId > 0).ToList();

        /// <summary>
        /// Adds a new buff to the list.
        /// </summary>
        /// <param name="buff">The new buff.</param>
        public void Add(DigimonBuffModel buff)
        {
            if (Buffs.Any(x => x.BuffId == buff.BuffId))
                return;

            // NOTE: v487 client has a same-class overlap rule (`DataMng.cpp:3895-3897`)
            // but it's narrowly scoped: only fires when a player TRIES TO USE a consumable
            // that would grant a system buff (`s_nBuffType == 3`), as a UI pre-check
            // before opening the apply-confirm dialog. Comparison is
            // `existing.MinLv <= incoming.MinLv → reject incoming` (lower-MinLv same-class
            // buff blocks the upgrade). Server-side anti-cheat for this belongs in
            // `ItemConsumePacketProcessor` (the only packet path that matches the client
            // gate), not here — applying it to every buff Add would reject legitimate
            // skill-cast / title-equip / evolution buff flows that the client never
            // overlap-checks. Deferred: enforce in item consume when system buffs come
            // through that path.
            Buffs.Add(buff);
        }

        /// <summary>
        /// Removes an active buff from the list.
        /// </summary>
        /// <param name="buffId">The target buff id.</param>
        public bool ForceExpired(int buffId)
        {
            var BuffToExpired = Buffs.FirstOrDefault(x => x.BuffId == buffId);

            if (BuffToExpired == null)
                return false;

            BuffToExpired.SetDuration(-1);
            BuffToExpired.SetEndDate(DateTime.Now.AddMinutes(1));

            return true;
        }

        public bool Remove(int buffId)
        {
            return Buffs.RemoveAll(x => x.BuffId == buffId) > 0;
        }

        public DigimonBuffModel TamerBaseSkill()
        {
            return Buffs.FirstOrDefault(x => x.SkillId / 1000000 == 8 && x.Duration == 0);
        }
        public DigimonBuffModel ActiveTamerSkill()
        {
            return Buffs.FirstOrDefault(x => x.SkillId / 1000000 == 8 && x.Cooldown >= 0);
        }

        /// <summary>
        /// Updates the target buff in buff list.
        /// </summary>
        /// <param name="buff">The buff to be updated.</param>
        public void Update(DigimonBuffModel buff) => Buffs.FirstOrDefault(x => x.Id == buff.Id)?.IncreaseDuration(buff.Duration);

        public bool HaveActiveSkill(int skillId)
        {
            var targetSkill = Buffs.FirstOrDefault(x => x.SkillId == skillId);

            if (targetSkill != null)
                return true;

                return false;
        }
        /// <summary>
        /// Serializes buffs list.
        /// </summary>
        public byte[] ToArray()
        {
            byte[] buffer;

            using (MemoryStream m = new())
            {
                foreach (var buff in Buffs)
                    m.Write(buff.ToArray(), 0, 12);

                buffer = m.ToArray();
            }

            return buffer;
        }
    }
}
