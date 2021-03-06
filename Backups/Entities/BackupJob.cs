using System;
using System.Collections.Generic;
using System.Linq;
using Backups.Algorithms;
using Backups.Repository;
using Backups.Tools;
using Newtonsoft.Json;

namespace Backups.Entities
{
    public class BackupJob
    {
        [JsonProperty("algorithm")]
        private readonly IStorageCreationAlgorithm _algorithm;
        [JsonProperty("jobObjects")]
        private readonly List<JobObject> _jobObjects;
        [JsonProperty("repository")]
        private readonly IRepository _repository;

        public BackupJob(
            IRepository repository,
            IStorageCreationAlgorithm algorithm,
            IReadOnlyCollection<JobObject> jobObjects = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            Id = Guid.NewGuid();
            _repository.CreateBackupJobRepository(Id);

            JobObject invalidJobObject = jobObjects?.FirstOrDefault(jobObject => _repository
                .CheckIfJobObjectExists(jobObject.FullName));

            if (invalidJobObject != null)
            {
                throw new BackupException($"Impossible to create Backup Job! Job object" +
                                          $" {invalidJobObject.FullName} doesn't exist!");
            }

            _jobObjects = jobObjects == null ? new List<JobObject>() : jobObjects.ToList();
            Backup = new Backup();
        }

        [JsonConstructor]
        private BackupJob(
            Guid id,
            IRepository repository,
            IStorageCreationAlgorithm algorithm,
            List<JobObject> jobObjects,
            Backup backup)
        {
            Id = id;
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            _jobObjects = jobObjects ?? throw new ArgumentNullException(nameof(jobObjects));
            Backup = backup ?? throw new ArgumentNullException(nameof(backup));
        }

        public Guid Id { get; }
        [JsonIgnore]
        public IReadOnlyCollection<JobObject> JobObjects => _jobObjects;
        public Backup Backup { get; }

        public void AddJobObject(JobObject jobObject)
        {
            if (!_repository.CheckIfJobObjectExists(jobObject.FullName))
                throw new BackupException($"Job object {jobObject.FullName} doesn't exist!");

            if (_jobObjects.Contains(jobObject))
                throw new BackupException($"{jobObject.FullName} already in this Backup Job!");

            _jobObjects.Add(jobObject);
        }

        public void DeleteJobObject(JobObject jobObject)
        {
            if (!_jobObjects.Remove(jobObject))
                throw new BackupException($"{jobObject.FullName} not in this Backup Job!");
        }

        public void CreateRestorePoint(DateTime? creationTime = null)
        {
            IReadOnlyCollection<Storage> storages = _algorithm.CreateStorages(
                _repository,
                JobObjects.Select(jobObject => jobObject.FullName).ToList(),
                Id);

            Backup.AddRestorePoint(
                new RestorePoint(storages, creationTime));
        }

        public void DeleteRestorePoint(RestorePoint restorePoint)
        {
            if (Backup.DeleteRestorePoint(restorePoint))
            {
                _repository.DeleteStorages(restorePoint.Storages.Select(storage => storage.FullName).ToList());
            }
        }
    }
}