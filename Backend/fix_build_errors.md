# Build Errors Fix Plan - Step 12

## Summary of Build Errors (41 errors)

### 1. AuthConstants not found (21 errors)
- PermissionService.cs: Lines 332-364 use `AuthConstants.Permission.*`
- AuthorizationService.cs: Lines 246, 249, 345
- **Fix**: Replace with `Permissions.*` class

### 2. Missing Properties in UserPermission entity (4 errors)
- Missing: RevokedAt, RevokedBy properties
- Lines: 271, 272, 306, 307 in PermissionService.cs

### 3. Type Conversion Errors (4 errors)
- Line 79: List<PermissionDetail> to List<string>
- Line 78: List<string> to List<ScopeConsentItem>

### 4. Missing Properties in ScopeConsentDetail (2 errors)
- Missing: Name, IsAlreadyConsented properties
- ConsentService.cs lines 64, 68

### 5. Missing Properties in ApplicationUser (2 errors)
- DateOfBirth property doesn't exist
- AuthorizationService.cs lines 230, 231

### 6. Missing Properties in ClientInfo (1 error)
- Missing Name property
- ConsentService.cs line 76

### 7. String Permission Extension (2 errors)
- Lines 249, 345: string.Permission extension doesn't exist

## Action Items:
1. Add missing properties to entities
2. Replace AuthConstants with Permissions
3. Fix type conversions
4. Comment out missing properties temporarily

Let's fix these systematically!
