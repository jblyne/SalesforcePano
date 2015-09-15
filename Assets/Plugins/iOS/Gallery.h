//
//  Gallery_iOS.h
//  Gallery_iOS
//
//  Created by Paul on 30/07/2015.
//  Copyright (c) 2015 Fluid Pixel. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <Photos/Photos.h>

@interface Gallery : NSObject<PHPhotoLibraryChangeObserver>

+ (Gallery* __nonnull) sharedInstance;

- (NSUInteger) getAssetCount;

- (NSString* __nullable) getLocalIdentifierForIndex: (NSUInteger) index;

- (PHAsset* __nullable) getAssetForLocalIdentifier: (NSString* __nonnull) localIdentifier;

- (NSString * __nonnull) getCountryForAssetLocation: (PHAsset* __nonnull) asset;

@end

