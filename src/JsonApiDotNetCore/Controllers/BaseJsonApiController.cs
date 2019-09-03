using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    public class BaseJsonApiController<T>
        : BaseJsonApiController<T, int>
        where T : class, IIdentifiable<int>
    {
        public BaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceService<T, int> resourceService
        ) : base(jsonApiContext, resourceService) { }

        public BaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceQueryService<T, int> queryService = null,
            IResourceCmdService<T, int> cmdService = null
        ) : base(jsonApiContext, queryService, cmdService) { }

        public BaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IGetAllService<T, int> getAll = null,
            IGetByIdService<T, int> getById = null,
            IGetRelationshipService<T, int> getRelationship = null,
            IGetRelationshipsService<T, int> getRelationships = null,
            ICreateService<T, int> create = null,
            IUpdateService<T, int> update = null,
            IUpdateRelationshipService<T, int> updateRelationships = null,
            IDeleteService<T, int> delete = null
        ) : base(jsonApiContext, getAll, getById, getRelationship, getRelationships, create, update, updateRelationships, delete) { }
    }

    public class BaseJsonApiController<T, TId>
        : JsonApiControllerMixin
        where T : class, IIdentifiable<TId>
    {
        private readonly IGetAllService<T, TId> _getAll;
        private readonly IGetByIdService<T, TId> _getById;
        private readonly IGetRelationshipService<T, TId> _getRelationship;
        private readonly IGetRelationshipsService<T, TId> _getRelationships;
        private readonly ICreateService<T, TId> _create;
        private readonly IUpdateService<T, TId> _update;
        private readonly IUpdateRelationshipService<T, TId> _updateRelationships;
        private readonly IDeleteService<T, TId> _delete;
        private readonly IJsonApiContext _jsonApiContext;

        public BaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceService<T, TId> resourceService)
        {
            _jsonApiContext = jsonApiContext.ApplyContext<T>(this);
            _getAll = resourceService;
            _getById = resourceService;
            _getRelationship = resourceService;
            _getRelationships = resourceService;
            _create = resourceService;
            _update = resourceService;
            _updateRelationships = resourceService;
            _delete = resourceService;
        }

        public BaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceQueryService<T, TId> queryService = null,
            IResourceCmdService<T, TId> cmdService = null)
        {
            _jsonApiContext = jsonApiContext.ApplyContext<T>(this);
            _getAll = queryService;
            _getById = queryService;
            _getRelationship = queryService;
            _getRelationships = queryService;
            _create = cmdService;
            _update = cmdService;
            _updateRelationships = cmdService;
            _delete = cmdService;
        }

        public BaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IGetAllService<T, TId> getAll = null,
            IGetByIdService<T, TId> getById = null,
            IGetRelationshipService<T, TId> getRelationship = null,
            IGetRelationshipsService<T, TId> getRelationships = null,
            ICreateService<T, TId> create = null,
            IUpdateService<T, TId> update = null,
            IUpdateRelationshipService<T, TId> updateRelationships = null,
            IDeleteService<T, TId> delete = null)
        {
            _jsonApiContext = jsonApiContext.ApplyContext<T>(this);
            _getAll = getAll;
            _getById = getById;
            _getRelationship = getRelationship;
            _getRelationships = getRelationships;
            _create = create;
            _update = update;
            _updateRelationships = updateRelationships;
            _delete = delete;
        }

        public virtual async Task<IActionResult> GetAsync()
        {
            if (_getAll == null) throw Exceptions.UnSupportedRequestMethod;

            var entities = await _getAll.GetAsync();

            return Ok(entities);
        }

        public virtual async Task<IActionResult> GetAsync(TId id)
        {
            if (_getById == null) throw Exceptions.UnSupportedRequestMethod;

            var entity = await _getById.GetAsync(id);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        public virtual async Task<IActionResult> GetRelationshipsAsync(TId id, string relationshipName)
        {
            if (_getRelationships == null) throw Exceptions.UnSupportedRequestMethod;

            var relationship = await _getRelationships.GetRelationshipsAsync(id, relationshipName);
            if (relationship == null)
                return NotFound();

            return Ok(relationship);
        }

        public virtual async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
        {
            if (_getRelationship == null) throw Exceptions.UnSupportedRequestMethod;

            var relationship = await _getRelationship.GetRelationshipAsync(id, relationshipName);

            return Ok(relationship);
        }

        public virtual async Task<IActionResult> PostAsync([FromBody] T entity)
        {
            if (_create == null)
                throw Exceptions.UnSupportedRequestMethod;

            if (entity == null)
                return UnprocessableEntity();

            if (!_jsonApiContext.Options.AllowClientGeneratedIds && !string.IsNullOrEmpty(entity.StringId))
                return Forbidden();

            if (_jsonApiContext.Options.ValidateModelState && !ModelState.IsValid)
                return UnprocessableEntity(ModelState.ConvertToErrorCollection<T>(_jsonApiContext.ResourceGraph));

            entity = await _create.CreateAsync(entity);

            return Created($"{HttpContext.Request.Path}/{entity.Id}", entity);
        }

        public virtual async Task<IActionResult> PatchAsync(TId id, [FromBody] T entity)
        {
            if (_update == null) throw Exceptions.UnSupportedRequestMethod;

            if (entity == null)
                return UnprocessableEntity();

            if (_jsonApiContext.Options.ValidateModelState && !ModelState.IsValid)
                return UnprocessableEntity(ModelState.ConvertToErrorCollection<T>(_jsonApiContext.ResourceGraph));

            var updatedEntity = await _update.UpdateAsync(id, entity);

            if (updatedEntity == null)
                return NotFound();

            return Ok(updatedEntity);
        }

        public virtual async Task<IActionResult> PatchRelationshipsAsync(TId id, string relationshipName, [FromBody] List<ResourceObject> relationships)
        {
            if (_updateRelationships == null) throw Exceptions.UnSupportedRequestMethod;

            await _updateRelationships.UpdateRelationshipsAsync(id, relationshipName, relationships);

            return Ok();
        }

        public virtual async Task<IActionResult> DeleteAsync(TId id)
        {
            if (_delete == null) throw Exceptions.UnSupportedRequestMethod;

            var wasDeleted = await _delete.DeleteAsync(id);

            if (!wasDeleted)
                return NotFound();

            return NoContent();
        }
    }

    public class GetsBaseJsonApiController<T, TId>
        : JsonApiControllerMixin
    where T : class, IIdentifiable<TId>
    {
        protected readonly IGetAllService<T, TId> _getAll;
        protected readonly IGetByIdService<T, TId> _getById;
        protected readonly IJsonApiContext _jsonApiContext;

        public GetsBaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceService<T, TId> resourceService)
        {
            _jsonApiContext = jsonApiContext.ApplyContext<T>(this);
            _getAll = resourceService;
            _getById = resourceService;
            //_getRelationship = resourceService;
            //_getRelationships = resourceService;
            //_create = resourceService;
            //_update = resourceService;
            //_updateRelationships = resourceService;
            //_delete = resourceService;
        }

        public GetsBaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceQueryService<T, TId> queryService = null)
        {
            _jsonApiContext = jsonApiContext.ApplyContext<T>(this);
            _getAll = queryService;
            _getById = queryService;
        }

        public GetsBaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IGetAllService<T, TId> getAll = null,
            IGetByIdService<T, TId> getById = null)
        {
            _jsonApiContext = jsonApiContext.ApplyContext<T>(this);
            _getAll = getAll;
            _getById = getById;
        }

        public virtual async Task<IActionResult> GetAsync()
        {
            if (_getAll == null) throw Exceptions.UnSupportedRequestMethod;

            var entities = await _getAll.GetAsync();

            return Ok(entities);
        }

        public virtual async Task<IActionResult> GetAsync(TId id)
        {
            if (_getById == null) throw Exceptions.UnSupportedRequestMethod;

            var entity = await _getById.GetAsync(id);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }
    }

    public class GetWithRelationshipsJsonApiController<T, TId>
    : GetsBaseJsonApiController<T, TId> where T : class, IIdentifiable<TId>
    {

        protected readonly IGetRelationshipService<T, TId> _getRelationship;
        protected readonly IGetRelationshipsService<T, TId> _getRelationships;

        public GetWithRelationshipsJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceService<T, TId> resourceService)
        : base(jsonApiContext, resourceService)
        {
            _getRelationship = resourceService;
            _getRelationships = resourceService;
        }

        public GetWithRelationshipsJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceQueryService<T, TId> queryService = null)
        : base(jsonApiContext, queryService)
        {
            _getRelationship = queryService;
            _getRelationships = queryService;
        }

        public GetWithRelationshipsJsonApiController(
           IJsonApiContext jsonApiContext,
            IGetAllService<T, TId> getAll = null,
            IGetByIdService<T, TId> getById = null,
            IGetRelationshipService<T, TId> getRelationship = null,
            IGetRelationshipsService<T, TId> getRelationships = null
        ) : base(jsonApiContext, getAll, getById)
        {
            _getRelationship = getRelationship;
            _getRelationships = getRelationships;
        }

        public virtual async Task<IActionResult> GetRelationshipsAsync(TId id, string relationshipName)
        {
            if (_getRelationships == null) throw Exceptions.UnSupportedRequestMethod;

            var relationship = await _getRelationships.GetRelationshipsAsync(id, relationshipName);
            if (relationship == null)
                return NotFound();

            return Ok(relationship);
        }

        public virtual async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
        {
            if (_getRelationship == null) throw Exceptions.UnSupportedRequestMethod;

            var relationship = await _getRelationship.GetRelationshipAsync(id, relationshipName);

            return Ok(relationship);
        }
    }

    public class PostPutDeleteBaseJsonApiController<T, TId>
        : GetsBaseJsonApiController<T, TId> where T : class, IIdentifiable<TId>
    {
        protected readonly ICreateService<T, TId> _create;
        protected readonly IUpdateService<T, TId> _update;
        protected readonly IDeleteService<T, TId> _delete;

        public PostPutDeleteBaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceService<T, TId> resourceService)
        : base(jsonApiContext, resourceService)
        {
            _create = resourceService;
            _update = resourceService;
            _delete = resourceService;
        }

        public PostPutDeleteBaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceQueryService<T, TId> queryService = null,
            IResourceCmdService<T, TId> cmdService = null)
        : base(jsonApiContext, queryService)
        {
            _create = cmdService;
            _update = cmdService;
            _delete = cmdService;
        }

        public PostPutDeleteBaseJsonApiController(
           IJsonApiContext jsonApiContext,
            IGetAllService<T, TId> getAll = null,
            IGetByIdService<T, TId> getById = null,
            ICreateService<T, TId> create = null,
            IUpdateService<T, TId> update = null,
            IDeleteService<T, TId> delete = null
        ) : base(jsonApiContext, getAll, getById)
        {

            _create = create;
            _update = update;
            _delete = delete;
        }
        public virtual async Task<IActionResult> PostAsync([FromBody] T entity)
        {
            if (_create == null)
                throw Exceptions.UnSupportedRequestMethod;

            if (entity == null)
                return UnprocessableEntity();

            if (!_jsonApiContext.Options.AllowClientGeneratedIds && !string.IsNullOrEmpty(entity.StringId))
                return Forbidden();

            if (_jsonApiContext.Options.ValidateModelState && !ModelState.IsValid)
                return UnprocessableEntity(ModelState.ConvertToErrorCollection<T>(_jsonApiContext.ResourceGraph));

            entity = await _create.CreateAsync(entity);

            return Created($"{HttpContext.Request.Path}/{entity.Id}", entity);
        }

        public virtual async Task<IActionResult> PatchAsync(TId id, [FromBody] T entity)
        {
            if (_update == null) throw Exceptions.UnSupportedRequestMethod;

            if (entity == null)
                return UnprocessableEntity();

            if (_jsonApiContext.Options.ValidateModelState && !ModelState.IsValid)
                return UnprocessableEntity(ModelState.ConvertToErrorCollection<T>(_jsonApiContext.ResourceGraph));

            var updatedEntity = await _update.UpdateAsync(id, entity);

            if (updatedEntity == null)
                return NotFound();

            return Ok(updatedEntity);
        }

        public virtual async Task<IActionResult> DeleteAsync(TId id)
        {
            if (_delete == null) throw Exceptions.UnSupportedRequestMethod;

            var wasDeleted = await _delete.DeleteAsync(id);

            if (!wasDeleted)
                return NotFound();

            return NoContent();
        }
    }
    public class FullBaseJsonApiController<T, TId>
        : PostPutDeleteBaseJsonApiController<T, TId> where T : class, IIdentifiable<TId>
    {

        protected readonly IGetRelationshipService<T, TId> _getRelationship;
        protected readonly IGetRelationshipsService<T, TId> _getRelationships;
        protected readonly IUpdateRelationshipService<T, TId> _updateRelationships;

        public FullBaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceService<T, TId> resourceService)
        : base(jsonApiContext, resourceService)
        {
            _getRelationship = resourceService;
            _getRelationships = resourceService;
            _updateRelationships = resourceService;
        }

        public FullBaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceQueryService<T, TId> queryService = null,
            IResourceCmdService<T, TId> cmdService = null)
        : base(jsonApiContext, queryService, cmdService)
        {
            _getRelationship = queryService;
            _getRelationships = queryService;
            _updateRelationships = cmdService;
        }

        public FullBaseJsonApiController(
           IJsonApiContext jsonApiContext,
            IGetAllService<T, TId> getAll = null,
            IGetByIdService<T, TId> getById = null,
            IGetRelationshipService<T, TId> getRelationship = null,
            IGetRelationshipsService<T, TId> getRelationships = null,
            ICreateService<T, TId> create = null,
            IUpdateService<T, TId> update = null,
            IUpdateRelationshipService<T, TId> updateRelationships = null,
            IDeleteService<T, TId> delete = null
        ) : base(jsonApiContext, getAll, getById, create, update, delete)
        {
            _getRelationship = getRelationship;
            _getRelationships = getRelationships;
            _updateRelationships = updateRelationships;
        }
        public virtual async Task<IActionResult> GetRelationshipsAsync(TId id, string relationshipName)
        {
            if (_getRelationships == null) throw Exceptions.UnSupportedRequestMethod;

            var relationship = await _getRelationships.GetRelationshipsAsync(id, relationshipName);
            if (relationship == null)
                return NotFound();

            return Ok(relationship);
        }

        public virtual async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
        {
            if (_getRelationship == null) throw Exceptions.UnSupportedRequestMethod;

            var relationship = await _getRelationship.GetRelationshipAsync(id, relationshipName);

            return Ok(relationship);
        }

        public virtual async Task<IActionResult> PatchRelationshipsAsync(TId id, string relationshipName, [FromBody] List<ResourceObject> relationships)
        {
            if (_updateRelationships == null) throw Exceptions.UnSupportedRequestMethod;

            await _updateRelationships.UpdateRelationshipsAsync(id, relationshipName, relationships);

            return Ok();
        }

    }
}
